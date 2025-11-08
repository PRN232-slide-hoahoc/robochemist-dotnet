using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using RoboChemist.SlidesService.Service.Interfaces;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;
using A = DocumentFormat.OpenXml.Drawing;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class PowerPointService : IPowerPointService
    {
        public void ImportDataToTemplate(ResponseGenerateDataDto data, Stream templateStream, string outputPath)
        {
            // Copy template stream to output file
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                templateStream.CopyTo(fileStream);
            }

            // Reset stream position if possible (for MemoryStream)
            if (templateStream.CanSeek)
            {
                templateStream.Position = 0;
            }

            using var presentationDoc = PresentationDocument.Open(outputPath, true);
            var presentationPart = presentationDoc.PresentationPart
                ?? throw new InvalidOperationException("Không tìm thấy PresentationPart trong template.");

            var slideIds = presentationPart.Presentation.SlideIdList?.Elements<SlideId>().ToList() 
                ?? throw new InvalidOperationException("Không tìm thấy SlideIdList trong template.");

            if (slideIds.Count < 4)
                throw new InvalidOperationException("Template cần có ít nhất 4 slide: First, TOC, Content, Thanks.");

            // Slide index mapping
            var firstSlidePart = (SlidePart)presentationPart.GetPartById(slideIds[0].RelationshipId!);
            var tocSlidePart = (SlidePart)presentationPart.GetPartById(slideIds[1].RelationshipId!);
            var thanksSlidePart = (SlidePart)presentationPart.GetPartById(slideIds[^1].RelationshipId!);

            // Select only content slide templates
            var contentTemplateParts = slideIds
                .Skip(2)
                .Take(slideIds.Count - 3)
                .Select(id => (SlidePart)presentationPart.GetPartById(id.RelationshipId!))
                .ToList();

            // --- First Slide ---
            FillFirstSlide(firstSlidePart, data.FirstSlide);

            // --- Table of Contents ---
            FillTableOfContentSlide(tocSlidePart, data.TableOfContentSlide);

            // --- Content Slides (random template) ---
            var random = new Random();
            foreach (var contentDto in data.ContentSlides)
            {
                var chosenTemplate = contentTemplateParts[random.Next(contentTemplateParts.Count)];
                var newSlidePart = CloneSlide(presentationPart, chosenTemplate);

                FillContentSlide(newSlidePart, contentDto);

                AddSlideBeforeLast(presentationPart, newSlidePart, thanksSlidePart);
            }

            // Remove unused content templates
            foreach (var templatePart in contentTemplateParts)
            {
                // get SlideId
                var slideId = slideIds.FirstOrDefault(id => id.RelationshipId == presentationPart.GetIdOfPart(templatePart));
                slideId?.Remove(); // remove SlideId from SlideIdList

                presentationPart.DeletePart(templatePart); // Remove slide part
            }

            presentationPart.Presentation.Save();
        }

        /// <summary>
        /// Populates the first slide with the specified title, subtitle, and owner information.
        /// </summary>
        /// <param name="slidePart">The slide part to be filled with content.</param>
        /// <param name="dto">An object containing the title, subtitle, and owner data to insert into the slide.</param>
        private static void FillFirstSlide(SlidePart slidePart, FisrtSlideTemplateDto dto)
        {
            ReplaceText(slidePart, "{{Title}}", dto.Title);
            ReplaceText(slidePart, "{{Subtitle}}", dto.Subtitle);
            ReplaceText(slidePart, "{{Owner}}", dto.Owner);
        }

        /// <summary>
        /// Populates the table of contents slide with the specified topics.
        /// </summary>
        /// <param name="slidePart">The slide part to be filled with content.</param>
        /// <param name="dto">The data transfer object containing the list of topics to include in the slide.</param>
        private static void FillTableOfContentSlide(SlidePart slidePart, TableOfContentSlideTemplateDto dto)
        {
            var joined = string.Join("\n• ", dto.Topics);
            ReplaceText(slidePart, "{{Topics}}", BeautifyChemicalFormulasInText("• " + joined));
        }

        /// <summary>
        /// Populates the content slide with the specified heading, bullet points and image description.
        /// </summary>
        /// <param name="slidePart">The slide part to be filled with content.</param>
        /// <param name="dto">The data transfer object containing heading, bullet points and image description</param>
        private static void FillContentSlide(SlidePart slidePart, ContentSlideTemplateDto dto)
        {
            ReplaceText(slidePart, "{{Heading}}", BeautifyChemicalFormulasInText(dto.Heading));
            
            // Build hierarchical bullet points text
            var bulletsText = BuildHierarchicalBulletText(dto.BulletPoints);
            ReplaceText(slidePart, "{{Bullets}}", BeautifyChemicalFormulasInText(bulletsText));

            if (!string.IsNullOrEmpty(dto.ImageDescription))
                ReplaceText(slidePart, "{{ImageDescription}}", BeautifyChemicalFormulasInText(dto.ImageDescription));
        }

        /// <summary>
        /// Build hierarchical bullet points text with proper indentation based on level
        /// </summary>
        /// <param name="bulletPoints">List of bullet points with hierarchical structure</param>
        /// <param name="parentLevel">Parent level for recursive calls (default 0)</param>
        /// <returns>Formatted text with hierarchical indentation</returns>
        private static string BuildHierarchicalBulletText(List<BulletPoint> bulletPoints, int parentLevel = 0)
        {
            if (bulletPoints == null || bulletPoints.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            for (int i = 0; i < bulletPoints.Count; i++)
            {
                var bullet = bulletPoints[i];
                
                // Calculate indentation based on level
                var indent = new string(' ', (bullet.Level - 1) * 4); // 4 spaces per level
                
                // Choose bullet symbol based on level
                var symbol = bullet.Level switch
                {
                    1 => "•", // Main point
                    2 => "◦", // Sub point
                    3 => "▪", // Sub-sub point
                    _ => "-"  // Deeper levels
                };

                sb.Append($"{indent}{symbol} {bullet.Content}");

                // Recursively process children
                if (bullet.Children != null && bullet.Children.Count > 0)
                {
                    sb.AppendLine(); // Xuống dòng trước khi vào children
                    var childrenText = BuildHierarchicalBulletText(bullet.Children, bullet.Level);
                    sb.Append(childrenText);
                    
                    // Nếu không phải mục cuối cùng, thêm xuống dòng để phân cách với mục tiếp theo
                    if (i < bulletPoints.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
                else
                {
                    // Nếu không có children, xuống dòng bình thường
                    if (i < bulletPoints.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Replace placeholder text in slide with actual value
        /// </summary>
        /// <param name="slidePart">SlidePart</param>
        /// <param name="placeholder">Placeholder text</param>
        /// <param name="value">Actual value</param>
        private static void ReplaceText(SlidePart slidePart, string placeholder, string value)
        {
            var texts = slidePart.Slide.Descendants<A.Text>();
            foreach (var text in from text in texts
                                 where text.Text.Contains(placeholder)
                                 select text)
            {
                text.Text = text.Text.Replace(placeholder, value ?? "");
            }

            slidePart.Slide.Save();
        }

        /// <summary>
        /// Clone a slide from template SlidePart
        /// </summary>
        /// <param name="presentationPart">PowerPoint PresentationPart</param>
        /// <param name="templateSlidePart">Template SlidePart</param>
        /// <returns></returns>
        private static SlidePart CloneSlide(PresentationPart presentationPart, SlidePart templateSlidePart)
        {
            var newSlidePart = presentationPart.AddNewPart<SlidePart>();
            templateSlidePart.Slide.Save(newSlidePart);

            // Copy relationships
            foreach (var rel in templateSlidePart.Parts)
            {
                newSlidePart.AddPart(rel.OpenXmlPart, rel.RelationshipId);
            }

            return newSlidePart;
        }

        /// <summary>
        /// Add new slide before the last slide (Thanks slide)
        /// </summary>
        /// <param name="presentationPart">PowerPoint PresentationPart</param>
        /// <param name="newSlidePart">New SlidePart that you want to add</param>
        /// <param name="thanksSlidePart">Last (Thanks) SlidePart</param>
        private static void AddSlideBeforeLast(PresentationPart presentationPart, SlidePart newSlidePart, SlidePart thanksSlidePart)
        {
            var slideIdList = presentationPart.Presentation.SlideIdList;

            uint maxSlideId = slideIdList!.Elements<SlideId>().Select(s => s.Id!.Value).DefaultIfEmpty(256u).Max();
            var newSlideId = new SlideId
            {
                Id = maxSlideId + 1,
                RelationshipId = presentationPart.GetIdOfPart(newSlidePart)
            };

            // Insert before slide Thanks
            var thanksSlideId = slideIdList.Elements<SlideId>().First(s => s.RelationshipId == presentationPart.GetIdOfPart(thanksSlidePart));
            slideIdList.InsertBefore(newSlideId, thanksSlideId);
        }

        /// <summary>
        /// Beautifies chemical formulas within a given text by converting numeric subscripts to subscript format.
        /// </summary>
        /// <param name="input">The input text containing chemical formulas to be beautified.</param>
        /// <returns>A string with chemical formulas formatted with subscript numbers. Returns the original input if it is null or empty.</returns>
        private static string BeautifyChemicalFormulasInText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Pattern 1: Match formulas with coefficient in parentheses like (n+1)H2O, (2n)CH4
            // (\([a-zA-Z0-9+\-]+\))  : coefficient in parentheses like (n+1), (2n)
            // ([A-Z][A-Za-z0-9()]*)  : chemical formula
            string pattern1 = @"(\([a-zA-Z0-9+\-]+\))([A-Z][A-Za-z0-9()]*)";
            
            input = Regex.Replace(input, pattern1, match =>
            {
                var coeffPart = match.Groups[1].Value;  // Keep (n+1) as is
                var formulaPart = match.Groups[2].Value;
                var formattedFormula = ToChemicalSubscript(formulaPart);
                return $"{coeffPart}{formattedFormula}";
            });

            // Pattern 2: Match formulas inside parentheses after compound names like "Methane (CH4)"
            // \(([A-Z][A-Za-z0-9()]*)\)  : formula inside parentheses
            string pattern2 = @"\(([A-Z][A-Za-z0-9()]+)\)(?=[\s:,.\)]|$)";
            
            input = Regex.Replace(input, pattern2, match =>
            {
                var formulaPart = match.Groups[1].Value;
                var formattedFormula = ToChemicalSubscript(formulaPart);
                return $"({formattedFormula})";
            });

            // Pattern 3: Match formulas with optional leading dash and coefficient
            // (?<![A-Za-z])  : not preceded by letter (to avoid matching inside words)
            // (-)?           : optional leading dash (for groups like -CH3, -OH)
            // (\d+)?         : optional leading coefficient (ex: "3" in "3H2")
            // ([A-Z][A-Za-z0-9()]*)
            //                : chemical formula starting with capital letter
            // (\([a-zA-Z0-9+\-]+\))?
            //                : optional phase or ion in (), like (g), (aq), (l), (s), (2-)
            string pattern3 = @"(?<![A-Za-z])(-)?(\d+)?([A-Z][A-Za-z0-9()]*)(\([a-zA-Z0-9+\-]+\))?(?![A-Za-z])";

            return Regex.Replace(input, pattern3, match =>
            {
                var dash = match.Groups[1].Value;       // optional dash
                var coeff = match.Groups[2].Value;      // optional coefficient
                var formula = match.Groups[3].Value;    // formula part
                var phase = match.Groups[4].Value;      // optional phase

                // Skip if formula is too short (likely not a chemical formula)
                if (formula.Length == 1 && string.IsNullOrEmpty(coeff) && string.IsNullOrEmpty(dash))
                    return match.Value;

                var formattedFormula = ToChemicalSubscript(formula);

                return $"{dash}{coeff}{formattedFormula}{phase}";
            });
        }

        /// <summary>
        /// Convert numbers in chemical formula to subscript characters
        /// </summary>
        /// <param name="formula">Raw fomular string</param>
        private static string ToChemicalSubscript(string formula)
        {
            if (string.IsNullOrEmpty(formula)) return formula;

            var subMap = new Dictionary<char, char>
            {
                ['0'] = '₀',
                ['1'] = '₁',
                ['2'] = '₂',
                ['3'] = '₃',
                ['4'] = '₄',
                ['5'] = '₅',
                ['6'] = '₆',
                ['7'] = '₇',
                ['8'] = '₈',
                ['9'] = '₉'
            };

            var sb = new StringBuilder();

            foreach (var ch in formula)
            {
                if (subMap.TryGetValue(ch, out var sub))
                    sb.Append(sub);
                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }
    }
}

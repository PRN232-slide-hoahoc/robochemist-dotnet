using System.Text;
using System.Text.RegularExpressions;

namespace RoboChemist.Shared.Common.Helpers
{
    /// <summary>
    /// Helper class for formatting chemical formulas with proper subscripts and superscripts
    /// </summary>
    public static class ChemicalFormulaHelper
    {
        /// <summary>
        /// Beautifies chemical formulas by converting plain text notation to proper chemical notation
        /// with subscripts, superscripts, arrows, and special symbols.
        /// 
        /// Supported conversions:
        /// - Ion charges: Ca(2+) → Ca²⁺, Cl(-) → Cl⁻
        /// - Complex ions: [Cu(NH3)4](2+) → [Cu(NH₃)₄]²⁺
        /// - Oxidation states: N(+5) → N⁺⁵, Mn(+7) → Mn⁺⁷
        /// - Polymers: (C6H10O5)n → (C₆H₁₀O₅)ₙ
        /// - Isotopes: 27Al13 → ²⁷Al₁₃
        /// - Electron configs: 1s2 2p6 → 1s² 2p⁶
        /// - Concentrations: [H(+)] → [H⁺], [SO4(2-)] → [SO₄²⁻]
        /// - Electrons: ne, 2e → ne⁻, 2e⁻
        /// - Arrows: -> → →, <=> → ⇌
        /// - Triple bonds: =- → ≡
        /// - Delta symbol: Delta → Δ
        /// - General formulas: H2O → H₂O, 2CO2 → 2CO₂
        /// </summary>
        /// <param name="input">Text containing chemical formulas in plain text format</param>
        /// <returns>Formatted text with proper chemical notation</returns>
        public static string BeautifyChemicalFormulas(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Step 1: Handle arrows (must be before other replacements)
            input = input.Replace("->", "→");
            input = input.Replace("<=>", "⇌");
            input = input.Replace("<=", "⇌");
            
            // Step 2: Handle triple bond notation
            input = Regex.Replace(input, @"=-", "≡");
            
            // Step 3: Handle Delta symbol
            input = Regex.Replace(input, @"\bDelta\b", "Δ", RegexOptions.IgnoreCase);

            // Step 4: Handle complex ions: [Cu(NH3)4](2+) → [Cu(NH₃)₄]²⁺
            string complexIonPattern = @"\[([^\]]+)\]\((\d+)([+-])\)";
            input = Regex.Replace(input, complexIonPattern, match =>
            {
                var formula = match.Groups[1].Value;
                var number = match.Groups[2].Value;
                var sign = match.Groups[3].Value;
                
                var formattedFormula = ToSubscript(formula);
                var superNumber = ToSuperscript(number);
                var superSign = sign == "+" ? "⁺" : "⁻";
                
                return $"[{formattedFormula}]{superNumber}{superSign}";
            });

            // Step 5: Handle oxidation states: N(+5) → N⁺⁵
            string oxidationPattern = @"([A-Z][a-z]?)\(([+-])(\d+)\)";
            input = Regex.Replace(input, oxidationPattern, match =>
            {
                var element = match.Groups[1].Value;
                var sign = match.Groups[2].Value;
                var number = match.Groups[3].Value;
                
                var superSign = sign == "+" ? "⁺" : "⁻";
                var superNumber = ToSuperscript(number);
                
                return $"{element}{superSign}{superNumber}";
            });

            // Step 6: Handle polymer notation: (C6H10O5)n → (C₆H₁₀O₅)ₙ
            string polymerPattern = @"\(([A-Z][A-Za-z0-9]+)\)n\b";
            input = Regex.Replace(input, polymerPattern, match =>
            {
                var formula = match.Groups[1].Value;
                var formattedFormula = ToSubscript(formula);
                return $"({formattedFormula})ₙ";
            });

            // Step 7: Handle isotope notation: 27Al13 → ²⁷Al₁₃
            string isotopePattern = @"\b(\d+)([A-Z][a-z]?)(\d+)\b";
            input = Regex.Replace(input, isotopePattern, match =>
            {
                var massNumber = match.Groups[1].Value;
                var element = match.Groups[2].Value;
                var atomicNumber = match.Groups[3].Value;
                
                var superMass = ToSuperscript(massNumber);
                var subAtomic = ToSubscript(atomicNumber);
                
                return $"{superMass}{element}{subAtomic}";
            });

            // Step 8: Handle electron configuration: 1s2 → 1s²
            string electronConfigPattern = @"\b(\d+)([spdf])(\d+)\b";
            input = Regex.Replace(input, electronConfigPattern, match =>
            {
                var shell = match.Groups[1].Value;
                var orbital = match.Groups[2].Value;
                var electrons = match.Groups[3].Value;
                
                var superElectrons = ToSuperscript(electrons);
                
                return $"{shell}{orbital}{superElectrons}";
            });

            // Step 9: Handle concentration with charge: [H(+)] → [H⁺]
            string concentrationPattern = @"\[([A-Z][A-Za-z0-9]*)\(([+-])\)\]";
            input = Regex.Replace(input, concentrationPattern, match =>
            {
                var species = match.Groups[1].Value;
                var sign = match.Groups[2].Value;
                
                var formattedSpecies = ToSubscript(species);
                var superSign = sign == "+" ? "⁺" : "⁻";
                
                return $"[{formattedSpecies}{superSign}]";
            });
            
            // Step 10: Handle concentration with number: [SO4(2-)] → [SO₄²⁻]
            string concentrationWithNumberPattern = @"\[([A-Z][A-Za-z0-9]*)\((\d+)([+-])\)\]";
            input = Regex.Replace(input, concentrationWithNumberPattern, match =>
            {
                var species = match.Groups[1].Value;
                var number = match.Groups[2].Value;
                var sign = match.Groups[3].Value;
                
                var formattedSpecies = ToSubscript(species);
                var superNumber = ToSuperscript(number);
                var superSign = sign == "+" ? "⁺" : "⁻";
                
                return $"[{formattedSpecies}{superNumber}{superSign}]";
            });

            // Step 11: Handle electron: ne, 2e → ne⁻, 2e⁻
            string electronPattern = @"\b(\d*)e\b(?!\-)";
            input = Regex.Replace(input, electronPattern, match =>
            {
                var coeff = match.Groups[1].Value;
                return $"{coeff}e⁻";
            });

            // Step 12: Handle formulas with coefficient in parentheses: (n+1)H2O
            string coefficientPattern = @"(\([a-zA-Z0-9+\-]+\))([A-Z][A-Za-z0-9()]*)";
            input = Regex.Replace(input, coefficientPattern, match =>
            {
                var coeffPart = match.Groups[1].Value;
                var formulaPart = match.Groups[2].Value;
                var formattedFormula = ToSubscript(formulaPart);
                return $"{coeffPart}{formattedFormula}";
            });

            // Step 13: Handle formulas inside parentheses (preserves state notations)
            string formulaInParenPattern = @"\(([A-Z][A-Za-z0-9()]+)\)(?=[\s:,.\)]|$)";
            input = Regex.Replace(input, formulaInParenPattern, match =>
            {
                var formulaPart = match.Groups[1].Value;
                
                // Skip state notations (g, s, l, aq, etc.)
                if (Regex.IsMatch(formulaPart, @"^[a-z]{1,3}$|^đặc$|^loãng$|^rắn$"))
                    return match.Value;
                
                var formattedFormula = ToSubscript(formulaPart);
                return $"({formattedFormula})";
            });

            // Step 14: Handle simple charges: Cu(2+), Fe(3+), Cl(-)
            string chargePattern = @"(?<!\[)([A-Z][a-z]?)\((\d*)([+-])\)";
            input = Regex.Replace(input, chargePattern, match =>
            {
                var element = match.Groups[1].Value;
                var number = match.Groups[2].Value;
                var sign = match.Groups[3].Value;
                
                var superNumber = string.IsNullOrEmpty(number) ? "" : ToSuperscript(number);
                var superSign = sign == "+" ? "⁺" : "⁻";
                
                return $"{element}{superNumber}{superSign}";
            });

            // Step 15: Handle general chemical formulas with optional coefficient
            string generalPattern = @"(?<![A-Za-z])(-)?(\d+)?\s*([A-Z][A-Za-z0-9()]*)(\([a-zA-Z0-9+\-]+\))?(?![A-Za-z])";
            return Regex.Replace(input, generalPattern, match =>
            {
                var dash = match.Groups[1].Value;
                var coeff = match.Groups[2].Value;
                var formula = match.Groups[3].Value;
                var phase = match.Groups[4].Value;

                // Skip if formula is too short or is a state notation
                if (formula.Length == 1 && string.IsNullOrEmpty(coeff) && string.IsNullOrEmpty(dash))
                    return match.Value;
                
                if (Regex.IsMatch(formula, @"^[a-z]{1,3}$"))
                    return match.Value;

                var formattedFormula = ToSubscript(formula);
                return $"{dash}{coeff}{formattedFormula}{phase}";
            });
        }

        /// <summary>
        /// Converts numbers in a string to Unicode subscript characters
        /// </summary>
        /// <param name="text">Text containing numbers to convert</param>
        /// <returns>Text with numbers converted to subscripts</returns>
        public static string ToSubscript(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var subscriptMap = new Dictionary<char, char>
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
            foreach (var ch in text)
            {
                if (subscriptMap.TryGetValue(ch, out var subscript))
                    sb.Append(subscript);
                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts numbers in a string to Unicode superscript characters
        /// </summary>
        /// <param name="text">Text containing numbers to convert</param>
        /// <returns>Text with numbers converted to superscripts</returns>
        public static string ToSuperscript(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var superscriptMap = new Dictionary<char, char>
            {
                ['0'] = '⁰',
                ['1'] = '¹',
                ['2'] = '²',
                ['3'] = '³',
                ['4'] = '⁴',
                ['5'] = '⁵',
                ['6'] = '⁶',
                ['7'] = '⁷',
                ['8'] = '⁸',
                ['9'] = '⁹'
            };

            var sb = new StringBuilder();
            foreach (var ch in text)
            {
                if (superscriptMap.TryGetValue(ch, out var superscript))
                    sb.Append(superscript);
                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }
    }
}

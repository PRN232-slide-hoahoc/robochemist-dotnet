using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.VNPayDTOs;

namespace RoboChemist.WalletService.Service.Interfaces
{
    public interface IVNPayService
    {
        Task<string> CreateDepositUrlAsync(DepositRequestDTO depositRequestDTO, HttpContext httpContext);
    }
}

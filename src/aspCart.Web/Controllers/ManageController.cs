﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using aspCart.Web.Models.ManageViewModels;
using aspCart.Infrastructure.EFModels;
using aspCart.Core.Interface.User;
using aspCart.Core.Domain.User;
using AutoMapper;

namespace aspCart.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IBillingAddressService _billingAddressService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IBillingAddressService billingAddressService,
            ILoggerFactory loggerFactory,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _billingAddressService = billingAddressService;
            _logger = loggerFactory.CreateLogger<ManageController>();
            _mapper = mapper;
        }

        //
        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
            };
            return View(model);
        }
        
        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        // GET: /Manage/EditBillingAddress
        public async Task<IActionResult> EditBillingAddress()
        {
            var user = await GetCurrentUserAsync();
            var billingAddressEntity = _billingAddressService.GetBillingAddressById(user.BillingAddressId);
            if (billingAddressEntity == null)
                return View();

            var billingAddressModel = _mapper.Map<BillingAddress, BillingAddressModel>(billingAddressEntity);

            return View(billingAddressModel);
        }

        // POST: /Manage/EditBillingAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBillingAddress(BillingAddressModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                Guid billingAddressId = Guid.NewGuid();

                // check if there are already billing address
                var billingAddressEntity = _billingAddressService.GetBillingAddressById(user.BillingAddressId);
                if (billingAddressEntity == null)
                {
                    billingAddressEntity = _mapper.Map<BillingAddressModel, BillingAddress>(model);
                    billingAddressEntity.Id = billingAddressId;

                    // save billing address in database
                    _billingAddressService.InsertBillingAddress(billingAddressEntity);
                }
                else
                {
                    billingAddressId = billingAddressEntity.Id;
                    billingAddressEntity = _mapper.Map<BillingAddressModel, BillingAddress>(model);
                    billingAddressEntity.Id = billingAddressId;

                    // update billing address in database
                    _billingAddressService.UpdateBillingAddress(billingAddressEntity);
                }


                // update user billing address
                user.BillingAddressId = billingAddressEntity.Id;
                await _userManager.UpdateAsync(user);
            }

            return View(model);
        }


        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            Error
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}

// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#pragma warning disable SA1600

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.Website.Pages
{
    public partial class ErrorModel : PageModel
    {
        public int ErrorStatusCode { get; set; } = StatusCodes.Status500InternalServerError;

        public void OnGet(int? id)
        {
            if (id.HasValue && id >= 400 && id < 599)
            {
                ErrorStatusCode = id.Value;
            }

            Response.StatusCode = ErrorStatusCode;
        }
    }
}

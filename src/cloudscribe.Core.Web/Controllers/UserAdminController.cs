﻿// Author:					Joe Audette
// Created:					2014-12-08
// Last Modified:			2014-12-10
// 

using cloudscribe.Configuration;
using cloudscribe.Core.Models;
using cloudscribe.Core.Web.ViewModels.UserAdmin;
using MvcSiteMapProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace cloudscribe.Core.Web.Controllers
{
    [Authorize(Roles = "Admins")]
    public class UserAdminController : CloudscribeBaseController
    {
        public async Task<ActionResult> Index(
            string query = "",
            int sortMode = 2,
            int pageNumber = 1, 
            int pageSize = -1,
            int siteId = -1)
        {
            ViewBag.SiteName = Site.SiteSettings.SiteName;
            ViewBag.Title = "User Management";
            //ViewBag.Heading = "Role Management";

            int itemsPerPage = AppSettings.DefaultPageSize_UserList;
            if (pageSize > 0)
            {
                itemsPerPage = pageSize;
            }

            int totalPages = 0;

            if(siteId != -1)
            {
                if(!Site.SiteSettings.IsServerAdminSite)
                {
                    siteId = Site.SiteSettings.SiteId;
                }
            }
            else
            {
                siteId = Site.SiteSettings.SiteId;
            }

            List<IUserInfo> siteMembers;
            //if(searchExp.Length > 0)
            //{
            //    siteMembers = Site.UserRepository.GetUserAdminSearchPage(
            //    siteId,
            //    pageNumber,
            //    pageSize,
            //    searchExp,
            //    sortMode, out totalPages);
            //}
            //else
            //{

            //}

            siteMembers = Site.UserRepository.GetPage(
                siteId,
                pageNumber,
                itemsPerPage,
                query,
                sortMode, 
                out totalPages);
             

            UserListViewModel model = new UserListViewModel();
            model.Heading = "User Management";
            model.UserList = siteMembers;
            model.Paging.CurrentPage = pageNumber;
            model.Paging.ItemsPerPage = itemsPerPage;
            model.Paging.TotalPages = totalPages;
            model.AlphaQuery = query; //TODO: sanitize

            return View(model);


        }

        public async Task<ActionResult> Search(
            string query = "",
            int sortMode = 2,
            int pageNumber = 1,
            int pageSize = -1,
            int siteId = -1)
        {
            ViewBag.SiteName = Site.SiteSettings.SiteName;
            ViewBag.Title = "User Management";
            //ViewBag.Heading = "Role Management";

            int itemsPerPage = AppSettings.DefaultPageSize_UserList;
            if (pageSize > 0)
            {
                itemsPerPage = pageSize;
            }

            int totalPages = 0;

            if (siteId != -1)
            {
                if (!Site.SiteSettings.IsServerAdminSite)
                {
                    siteId = Site.SiteSettings.SiteId;
                }
            }
            else
            {
                siteId = Site.SiteSettings.SiteId;
            }

            List<IUserInfo> siteMembers;
            //if(searchExp.Length > 0)
            //{
            //    siteMembers = Site.UserRepository.GetUserAdminSearchPage(
            //    siteId,
            //    pageNumber,
            //    pageSize,
            //    searchExp,
            //    sortMode, out totalPages);
            //}
            //else
            //{

            //}

            siteMembers = Site.UserRepository.GetUserAdminSearchPage(
                siteId,
                pageNumber,
                itemsPerPage,
                query,
                sortMode,
                out totalPages);


            UserListViewModel model = new UserListViewModel();
            model.Heading = "User Management";
            model.UserList = siteMembers;
            model.Paging.CurrentPage = pageNumber;
            model.Paging.ItemsPerPage = itemsPerPage;
            model.Paging.TotalPages = totalPages;
            model.SearchQuery = query; //TODO: sanitize
            

            return View("Index", model);


        }

        public async Task<ActionResult> IpSearch(string ipQuery = "",int siteId = -1)
        {
            ViewBag.SiteName = Site.SiteSettings.SiteName;
            ViewBag.Title = "User Management";
            //ViewBag.Heading = "Role Management";

            
            Guid siteGuid = Site.SiteSettings.SiteGuid;

            if (siteId != -1)
            {
                if (Site.SiteSettings.IsServerAdminSite)
                {
                    ISiteSettings otherSite = Site.SiteRepository.Fetch(siteId);
                    if(otherSite != null)
                    {
                        siteGuid = otherSite.SiteGuid;
                    }
                }
            }
            

            List<IUserInfo> siteMembers;
            //if(searchExp.Length > 0)
            //{
            //    siteMembers = Site.UserRepository.GetUserAdminSearchPage(
            //    siteId,
            //    pageNumber,
            //    pageSize,
            //    searchExp,
            //    sortMode, out totalPages);
            //}
            //else
            //{

            //}

            siteMembers = Site.UserRepository.GetByIPAddress(
                siteGuid,
                ipQuery);


            UserListViewModel model = new UserListViewModel();
            model.Heading = "User Management";
            model.UserList = siteMembers;
            //model.Paging.CurrentPage = pageNumber;
            //model.Paging.ItemsPerPage = itemsPerPage;
            model.Paging.TotalPages = 1;
            model.IpQuery = ipQuery; //TODO: sanitize
            model.ShowAlphaPager = false;

            return View("Index", model);


        }

    }
}

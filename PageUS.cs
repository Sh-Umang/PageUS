using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace USPage
{
    public class PageUS
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        private int _TotalCount { get; set; }
        public bool NextPage { get; set; }
        public int LastPage { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public PageUS(int pageNo, int pageSize)
        {
            CurrentPage = pageNo;
            PageSize = pageSize;
            CalculateSkipTake(pageNo, pageSize);
        }
        public void CalculateSkipTake(int pageNo, int pageSize)
        {
            Skip = (pageNo - 1) * pageSize;
            Take = pageSize + 1;
        }
        public int TotalCount
        {
            get { return _TotalCount; }
            set
            {
                _TotalCount = value;
                if (PageSize > 0 && _TotalCount > 0)
                {
                    LastPage = (int)Math.Ceiling((double)_TotalCount / PageSize);
                }
            }
        }
    }
    public static class PageUSList
    {
        public static IEnumerable<T> ToPageUSList<T>(this IEnumerable<T> List, PageUS pg)
        {
            pg.NextPage = List.Count() >= pg.Take ? true : false;
            return List.Take(pg.PageSize);
        }
    }
    public static class PageUSHelper
    {
        public static MvcHtmlString AjaxPagination(this AjaxHelper ajaxHelper, PageUS pg, Func<int, string> generateUrl, AjaxOptions ajaxOptions)
        {
            int startPage = Math.Max(1, pg.CurrentPage - 1);
            int endPage = pg.NextPage ? pg.CurrentPage + 1 : pg.CurrentPage;

            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination-custom");

            var sbHtml = new StringBuilder();

            // Previous
            AppendPage(sbHtml, pg.CurrentPage - 1, isDisabled: pg.CurrentPage == 1, text: "❮");
            if (startPage >= 2)
            {
                AppendPage(sbHtml, 1);
            }
            //int endPage = Math.Min(totalPages, currentPage + 1);

            if (startPage > 2)
            {
                //AppendPage(sbHtml, 1);
                sbHtml.Append("<li class='dots'>...</li>");
            }

            for (int i = startPage; i <= endPage; i++)
                AppendPage(sbHtml, i, isActive: i == pg.CurrentPage);

            if (pg.NextPage && (pg.CurrentPage + 1) != pg.LastPage)
            {
                sbHtml.Append("<li class='dots'>...</li>");
            }

            // Next
            AppendPage(sbHtml, pg.CurrentPage + 1, isDisabled: !pg.NextPage, text: "❯");

            // Last Page
            if (pg.LastPage > 0)
            {
                AppendPage(sbHtml, pg.LastPage, isDisabled: !pg.NextPage, text: "❯❯");
            }

            ul.InnerHtml = sbHtml.ToString();
            return MvcHtmlString.Create(ul.ToString());

            void AppendPage(StringBuilder sb, int page, bool isActive = false, bool isDisabled = false, string text = null)
            {
                var li = new TagBuilder("li");
                li.AddCssClass("page-item");

                if (isActive)
                    li.AddCssClass("active");
                if (isDisabled)
                    li.AddCssClass("disabled");

                text = text ?? page.ToString();

                var link = isDisabled
                    ? (new TagBuilder("span") { InnerHtml = text }).ToString()
                    : (ajaxHelper.ActionLink(text, null, null, ajaxOptions, new { href = generateUrl(page) })).ToHtmlString();

                li.InnerHtml = isDisabled ? $"<span>{text}</span>" : link;
                sb.Append(li.ToString());
            }
        }

    }
}

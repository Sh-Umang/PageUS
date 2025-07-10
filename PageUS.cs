using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Linq.Expressions;

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
    public class PageUSResult<T>
    {
        public PageUS pageUS { get; set; }
        public IEnumerable<T> pageUSList { get; set; }
    }
    public static class PageUSList
    {
        private static bool IsOrdered<T>(this IQueryable<T> query)
        {
            var expression = query.Expression.ToString();
            return expression.Contains("OrderBy") || expression.Contains("OrderByDescending");
        }
        private static IQueryable<T> ApplyOrderIfMissing<T>(this IQueryable<T> source)
        {
            if (source.IsOrdered())
                return source;

            // Default to first scalar property (e.g., int, string, DateTime, etc.)
            var type = typeof(T);
            var firstProp = type.GetProperties()
                                .FirstOrDefault(p => Type.GetTypeCode(p.PropertyType) != TypeCode.Object);

            if (firstProp == null)
                throw new InvalidOperationException($"Type {type.Name} has no sortable public properties.");

            var parameter = Expression.Parameter(type, "x");
            var propertyAccess = Expression.Property(parameter, firstProp);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var result = Expression.Call(
                typeof(Queryable),
                "OrderBy",
                new[] { type, firstProp.PropertyType },
                source.Expression,
                Expression.Quote(lambda)
            );

            return source.Provider.CreateQuery<T>(result);
        }
        public static IEnumerable<T> ToPageUSList<T>(this IEnumerable<T> List, PageUS pg)
        {
            pg.NextPage = List.Count() >= pg.Take ? true : false;
            return List.Take(pg.PageSize);
        }
        public static PageUSResult<T> ToPageUSResult<T>(this IQueryable<T> List, int page, int pageSize)
        {
            var pageUS = new PageUS(page, pageSize);
            pageUS.TotalCount = List.Count();
            List = List.ApplyOrderIfMissing();
            return new PageUSResult<T>
            {
                pageUS = pageUS,
                pageUSList = List.Skip(pageUS.Skip).Take(pageUS.Take).ToPageUSList(pageUS)
            };
        }
    }
    public static class PageUSHelper
    {
        public static MvcHtmlString AjaxPagination(this AjaxHelper ajaxHelper, PageUS pg, Func<int, string> generateUrl, AjaxOptions ajaxOptions)
        {
            return GeneratePagination(ajaxHelper, pg, generateUrl, ajaxOptions);
        }
        public static MvcHtmlString AjaxPagination<T>(this AjaxHelper ajaxHelper, PageUSResult<T> pgResult, Func<int, string> generateUrl, AjaxOptions ajaxOptions)
        {
            var pg = pgResult.pageUS;
            return GeneratePagination(ajaxHelper, pg, generateUrl, ajaxOptions);
        }

        private static MvcHtmlString GeneratePagination(AjaxHelper ajaxHelper, PageUS pg, Func<int, string> generateUrl, AjaxOptions ajaxOptions)
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

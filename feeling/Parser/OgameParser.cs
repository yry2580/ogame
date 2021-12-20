using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !NET45
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
#else
using HtmlAgilityPack;
#endif
namespace feeling
{
    class OgameParser
    {
#if !NET45
        AngleSharp.Html.Parser.HtmlParser mParser = new AngleSharp.Html.Parser.HtmlParser();
        IHtmlDocument mDoc;
#else
        HtmlDocument hParser = new HtmlDocument();
        HtmlNode hDoc;
#endif
        public void LoadHtml(string html)
        {
#if !NET45
            mDoc = mParser?.ParseDocument(html);
#else
            hParser?.LoadHtml(html);
            hDoc = hParser?.DocumentNode;
#endif
        }

#if !NET45
        public IElement QuerySelector(string selectors)
        {
            if (null == mDoc) return null;
            return mDoc.QuerySelector(selectors);
        }

        public IHtmlCollection<IElement> QuerySelectorAll(string selectors)
        {
            if (null == mDoc) return null;
            return mDoc.QuerySelectorAll(selectors);
        }
#else
        public HtmlNode QuerySelector(string selectors)
        {
            if (null == hDoc) return null;
            return hDoc?.SelectSingleNode(selectors);
        }

        public HtmlNodeCollection QuerySelectorAll(string selectors)
        {
            if (null == hDoc) return null;
            return hDoc.SelectNodes(selectors);
        }
#endif
    }
}

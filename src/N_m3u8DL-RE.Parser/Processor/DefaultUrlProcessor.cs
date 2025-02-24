using N_m3u8DL_RE.Common.Enum;
using N_m3u8DL_RE.Common.Log;
using N_m3u8DL_RE.Parser.Config;
using System.Web;
using System.Text.RegularExpressions;

namespace N_m3u8DL_RE.Parser.Processor;

public class DefaultUrlProcessor : UrlProcessor
{
    public override bool CanProcess(ExtractorType extractorType, string oriUrl, ParserConfig parserConfig) => parserConfig.AppendUrlParams;

    public override string Process(string oriUrl, ParserConfig parserConfig)
    {
        if (!oriUrl.StartsWith("http")) return oriUrl;
        
        var uriFromConfig = new Uri(parserConfig.Url);
        var uriFromConfigQuery = HttpUtility.ParseQueryString(uriFromConfig.Query);

        var oldUri = new Uri(oriUrl);
        var newQuery = HttpUtility.ParseQueryString(oldUri.Query);
        foreach (var item in uriFromConfigQuery.AllKeys)
        {
            if (newQuery.AllKeys.Contains(item))
                newQuery.Set(item, uriFromConfigQuery.Get(item));
            else
                newQuery.Add(item, uriFromConfigQuery.Get(item));
        }

        if (string.IsNullOrEmpty(newQuery.ToString())) return oriUrl;
        
        Logger.Debug("Before: " + oriUrl);
        oriUrl = (oldUri.GetLeftPart(UriPartial.Path) + "?" + newQuery).TrimEnd('?');
        Logger.Debug("After: " + oriUrl);

        return oriUrl;
    }
}

public class UrlReplaceProcessor : UrlProcessor
{
    public override bool CanProcess(ExtractorType extractorType, string oriUrl, ParserConfig parserConfig) => parserConfig.UrlReplaceRegexs.Count > 0;

    public override string Process(string oriUrl, ParserConfig parserConfig)
    {
        foreach(KeyValuePair<string, string> entry in parserConfig.UrlReplaceRegexs)
        {
            oriUrl = Regex.Replace(oriUrl, entry.Key, entry.Value);
        }
        return oriUrl;
    }
}

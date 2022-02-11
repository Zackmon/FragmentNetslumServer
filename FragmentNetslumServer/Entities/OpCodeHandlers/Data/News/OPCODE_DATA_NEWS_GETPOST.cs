using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.News;

[OpCodeData(OpCodes.OPCODE_DATA_NEWS_GETPOST), Description("Get the image for the requested article")]
public sealed class OPCODE_DATA_NEWS_GETPOST : IOpCodeHandler
{
    private readonly INewsService _newsService;
    private readonly Encoding _encoding;
    
    public OPCODE_DATA_NEWS_GETPOST(INewsService newsService)
    {
        _newsService = newsService;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _encoding = Encoding.GetEncoding("Shift-JIS");
        
    }
    public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
    {
        List<ResponseContent> listOfResponse = new List<ResponseContent>();
        
        var articleId = BitConverter.ToUInt16(request.Data, 0).Swap();

        var article =  (await _newsService.GetNewsArticles()).First(a => a.ArticleID == articleId);

        if (article.ImageSizeInfo == null || article.ImageDetails == null)
        {
             listOfResponse.Add(request.CreateResponse(0x7857, new byte[] {0x00,0x00 })); // Error while getting the image data
        }
        else
        {
             listOfResponse.Add(request.CreateResponse(0x7855, article.ImageSizeInfo)); // send the image size and chunk count
             listOfResponse.Add(request.CreateResponse(0x7856, article.ImageDetails)); // send the color pallets and the image indices 
        }

        await _newsService.UpdateNewsLog(_encoding.GetString(request.Client.save_id), articleId);

        return listOfResponse;
    }
}
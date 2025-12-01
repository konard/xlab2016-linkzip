using Microsoft.AspNetCore.Mvc;
using Linkzip.Models;
using Linkzip.Services;

namespace Linkzip.Controllers;

[ApiController]
[Route("api/v1/zipper")]
public class ZipperController : ControllerBase
{
    private readonly ZipperService _zipperService;

    public ZipperController(ZipperService zipperService)
    {
        _zipperService = zipperService;
    }

    /// <summary>
    /// Converts text to compressed links notation.
    /// </summary>
    /// <param name="request">Text to compress</param>
    /// <returns>Compressed links notation</returns>
    [HttpPost("zip")]
    public ActionResult<ZipResponse> Zip([FromBody] ZipRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "Text cannot be empty" });
        }

        var (linksNotation, patternsApplied) = _zipperService.Zip(request.Text);

        return Ok(new ZipResponse
        {
            LinksNotation = linksNotation,
            PatternsApplied = patternsApplied
        });
    }

    /// <summary>
    /// Converts compressed links notation back to text.
    /// </summary>
    /// <param name="request">Links notation to decompress</param>
    /// <returns>Decompressed text</returns>
    [HttpPost("unzip")]
    public ActionResult<UnzipResponse> Unzip([FromBody] UnzipRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LinksNotation))
        {
            return BadRequest(new { error = "Links notation cannot be empty" });
        }

        var text = _zipperService.Unzip(request.LinksNotation);

        return Ok(new UnzipResponse
        {
            Text = text
        });
    }
}

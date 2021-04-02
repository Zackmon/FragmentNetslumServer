using FragmentServerWV.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Services.Interfaces
{

    /// <summary>
    /// Defines a service for interacting with the mail
    /// </summary>
    public interface IMailService: IBaseService
    {

        /// <summary>
        /// Asynchronously saves (and subsequently sends) all Mail information to the database
        /// </summary>
        /// <param name="content">The mail content</param>
        /// <returns>A promise to save the mail</returns>
        Task SaveMailAsync(byte[] content);

        /// <summary>
        /// Retrieves mail for the given account ID
        /// </summary>
        /// <param name="accountId">The player account ID</param>
        /// <returns>A promise that, when awaited, will return a collection of <see cref="MailMetaModel"/></returns>
        Task<IList<MailMetaModel>> GetMailAsync(int accountId);

        /// <summary>
        /// Retrieves the message body for a given piece of mail
        /// </summary>
        /// <param name="mailId">The ID for the piece of mail</param>
        /// <returns><see cref="MailBodyModel"/></returns>
        Task<MailBodyModel> GetMailContent(int mailId);

        /// <summary>
        /// Converts a received <see cref="MailMetaModel"/> into a transmittable byte array
        /// </summary>
        /// <param name="mail"><see cref="MailMetaModel"/></param>
        /// <returns>A promise to convert the <see cref="MailMetaModel"/> to a byte array</returns>
        Task<byte[]> ConvertMailMetaIntoBytes(MailMetaModel mail);

        /// <summary>
        /// Converts a received <see cref="MailBodyModel"/> into a transmittable byte array
        /// </summary>
        /// <param name="mail"><see cref="MailBodyModel"/></param>
        /// <returns>A promise to convert the <see cref="MailBodyModel"/> to a byte array</returns>
        Task<byte[]> ConvertMailBodyIntoBytes(MailBodyModel mail);

    }

}

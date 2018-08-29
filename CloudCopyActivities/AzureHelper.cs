using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.File;

namespace CloudCopyActivities
{
    class AzureHelper
    {       
        /// <summary>
        /// Upload a file to azure storage as a cloud file.
        /// </summary>
        public static async Task FileUploadToAzure(string sourceFileFullPath, string cloudFileName, string accountName, string accountKey,string shareName)
        {            
            // Create the destination CloudFile instance           
            CloudFile destinationFile = await Util.GetCloudFileAsync(shareName, cloudFileName, accountName,accountKey);
            // Start the async upload
            await TransferManager.UploadAsync(sourceFileFullPath, destinationFile);           
           
        }

        /// <summary>
        /// Download a cloud file from azure storage to local folder.
        /// </summary>
        public static async Task FileDownloadFromAzure(string destinationFileFullPath, string cloudFileName, string accountName, string accountKey, string shareName)
        {
            // Create the destination CloudFile instance           
            CloudFile sourceCloudFile = await Util.GetCloudFileAsync(shareName, cloudFileName,accountName,accountKey);

            // Start the async download
            await TransferManager.DownloadAsync(sourceCloudFile, destinationFileFullPath);            
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Activities;
using System.ComponentModel;

namespace CloudCopyActivities
{
    [DisplayName("Cloud Copy (Azure|AWS S3)")]
    [Description("UiPath Customer Activity that provide upload/download file to Azure/AWS S3 cloud storage.")]
    public class CloudCopyActivity:AsyncCodeActivity
    {
        public CloudCopyActivity() : base()
        {
        }
        public delegate Task asyncAzureFileDownloadFunc(string fileFullPath, string cloudFileName, string strAccountName, string strAccountKey, string strShareName);
        public delegate Task asyncAzureFileUploadFunc(string fileFullPath, string cloudFileName,string accountName,string accountKey,string shareName);


        public delegate Task asyncS3FileDownloadFunc(string destinationFileFullpath,string bucketName, string keyName, string accountName, string accountKey, string region);
        public delegate Task asyncS3FileUploadFunc(string bucketName, string keyName, string sourceFileFullPath, string accountName, string accountKey, string awsRegionName);

        [Category("Input")]
        [DisplayName("Source File Name (Full Path)")]
        [Description("Please specify full path (including file's name) of the file to be copy to Azure/S3.")]
        [RequiredArgument]
        public InArgument<string> sourceFullFilePath { get; set; }

        [Category("Input")]
        [DisplayName("Destination File Name")]
        [Description("Please specify name of the cloud file for upload, full path for download, for example (rpa-output.xlsx)")]
        [RequiredArgument]
        public InArgument<string> fileName { get; set; }

        [Category("Input")]
        [DisplayName("Azure's Account Name (e.g.: mycloudfolderxxxx) \n S3 AWS Access KeyId (e.g.: NPDGGFVJXE3IFJWVNQXA)")]
        [Description("Please specify the Azure Account Name or the AWS Access KeyId.")]
        [RequiredArgument]
        public InArgument<string> accountName { get; set; }

        [Category("Input")]
        [DisplayName("Azure's Account Key (e.g.: gJVTdJ3MBkDxxxxx) \n S3 AWS Secret Access Key (e.g.: wAOYg+z8O2Dr)")]
        [Description("Please specify the Azure Account Key or the AWS Secret Access Key.")]
        [RequiredArgument]
        public InArgument<string> accountKey { get; set; }

        [Category("Input")]
        [DisplayName("Azure Share name| AWS S3 Bucket name")]
        [Description("Please specify Azure Share name or AWS S3 Bucket name.")]
        [RequiredArgument]
        public InArgument<string> shareName { get; set; }

        [Category("Input")]
        [DisplayName("AWS Region (e.g.: USEast1)")]
        [Description("Please specify AWS Region name.")]
        [RequiredArgument]
        public InArgument<string> awsRegionName { get; set; }

        [Category("Input")]
        [DisplayName("Cloud Provider (e.g.: AWS|Azure)")]
        [Description("Please specify AWS or Azure.")]
        [RequiredArgument]
        public InArgument<string> cloudProviderName { get; set; }


        [Category("Input")]
        [DisplayName("Action type (e.g.: download|upload)")]
        [Description("Please specify action type as download or upload.")]
        [RequiredArgument]
        public InArgument<string> actionType { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            //Get the properties which are configured in UiPath custom activity
            string strSourceFullFilePath = sourceFullFilePath.Get(context);
            string strAccountName = accountName.Get(context);
            string strAccountKey = accountKey.Get(context);
            string strDestinationFileName = fileName.Get(context);
            string strShareName = shareName.Get(context);
            string strAwsRegionName = awsRegionName.Get(context);
            string strCloudProviderName = cloudProviderName.Get(context);
            string strActionType = actionType.Get(context);//download or upload action
            //end Get the properties which are configured in UiPath custom activity

            if (strCloudProviderName.ToLower().Equals("aws"))
            {
                //Using AWSHelper - AWS S3
                if (strActionType.ToLower().Equals("download"))
                {
                    asyncS3FileDownloadFunc fileDownloadFromS3Func = new asyncS3FileDownloadFunc(downloadFileFromS3);
                    context.UserState = fileDownloadFromS3Func;
                    return fileDownloadFromS3Func.BeginInvoke(strDestinationFileName, strShareName, strSourceFullFilePath, strAccountName,strAccountKey,strAwsRegionName, callback, state);
                }
                else if (strActionType.ToLower().Equals("upload"))
                {
                    asyncS3FileUploadFunc fileUploadToS3Func = new asyncS3FileUploadFunc(uploadFileToS3);
                    context.UserState = fileUploadToS3Func;
                    return fileUploadToS3Func.BeginInvoke(strShareName, strDestinationFileName, strSourceFullFilePath,strAccountName,strAccountKey, strAwsRegionName, callback, state);
                }
            }//end if AWS

            if (strCloudProviderName.ToLower().Equals("azure"))
            {
                //using AzureHelper - Azure 
                if (strActionType.ToLower().Equals("download"))
                {
                    asyncAzureFileDownloadFunc fileDownloadFunc = new asyncAzureFileDownloadFunc(downloadFileFromAzure);
                    context.UserState = fileDownloadFunc;
                    return fileDownloadFunc.BeginInvoke(strSourceFullFilePath, strDestinationFileName,strAccountName,strAccountKey,strShareName, callback, state);
                }
                else if (strActionType.ToLower().Equals("upload"))
                {
                    asyncAzureFileUploadFunc fileUploadFunc = new asyncAzureFileUploadFunc(uploadFileToAzure);
                    context.UserState = fileUploadFunc;
                    return fileUploadFunc.BeginInvoke(strSourceFullFilePath, strDestinationFileName,strAccountName,strAccountKey,strShareName, callback, state);
                }
            }//end if Azure  

            //default: do nothing if specify not AWS or Azure as cloud provider in the UIPath customer activity.          T
            return Task.Run(() => DoNothing());

        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            //Get the properties which are configured in UiPath custom activity
            string strSourceFullFilePath = sourceFullFilePath.Get(context);
            string strAccountName = accountName.Get(context);
            string strAccountKey = accountKey.Get(context);
            string strDestinationFileName = fileName.Get(context);
            string strShareName = shareName.Get(context);
            string strAwsRegionName = awsRegionName.Get(context);
            string strCloudProviderName = cloudProviderName.Get(context);
            string strActionType = actionType.Get(context);//download or upload action
            //end Get the properties which are configured in UiPath custom activity


            if (strCloudProviderName.ToLower().Equals("azure"))
            {
                //Azure: get the delegate from the UserState and call EndInvoke   
                if (strActionType.ToLower().Equals("download"))
                {
                    asyncAzureFileDownloadFunc fileDownloadFunc = (asyncAzureFileDownloadFunc)context.UserState;
                    fileDownloadFunc.EndInvoke(result);
                }
                else if (strActionType.ToLower().Equals("upload"))
                {
                    asyncAzureFileUploadFunc fileUploadFunc = (asyncAzureFileUploadFunc)context.UserState;
                    fileUploadFunc.EndInvoke(result);
                }
            }//end if Azure

            //AWS S3:
            if (strCloudProviderName.ToLower().Equals("aws"))
            {
                if (strActionType.ToLower().Equals("download"))
                {
                    asyncS3FileDownloadFunc fileDownloadFunc = (asyncS3FileDownloadFunc)context.UserState;
                    fileDownloadFunc.EndInvoke(result);
                }
                else if (strActionType.ToLower().Equals("upload"))
                {
                    asyncS3FileUploadFunc fileUploadFunc = (asyncS3FileUploadFunc)context.UserState;
                    fileUploadFunc.EndInvoke(result);
                }
            }//end if AWS
        }//end EndExecute

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            // Implement any cleanup as a result of the asynchronous work
            // being canceled, and then call MarkCanceled.
            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }



        private async Task uploadFileToAzure(string fileFullPath, string cloudFileName, string accountName,string accountKey,string shareName)
        {
            //await AzureHelper.FileUploadToAzure(fileFullPath, cloudFileName, strAccountName, strAccountKey, strShareName);
            await AzureHelper.FileUploadToAzure(fileFullPath, cloudFileName, accountName, accountKey, shareName);
        }

        private async Task downloadFileFromAzure(string fileFullPath, string cloudFileName,string strAccountName, string strAccountKey,string strShareName)
        {
            await AzureHelper.FileDownloadFromAzure(fileFullPath, cloudFileName, strAccountName, strAccountKey, strShareName);
        }

        private async Task downloadFileFromS3(string destinationFileFullPath,string bucketName,string keyName,string accountName,string accountKey,string region)
        {
            await AWSHelper.FileDownloadFromS3(destinationFileFullPath,bucketName, keyName, accountName, accountKey, region);
        }

        private async Task uploadFileToS3(string bucketName, string keyName,string sourceFileFullPath,string accountName, string accountKey, string region)
        {
            await AWSHelper.FileUploadToS3(bucketName, keyName, sourceFileFullPath, accountName, accountKey, region);
        }

        private void DoNothing()
        {
            //do nothing;
        }

    }//end class
}//end namespace

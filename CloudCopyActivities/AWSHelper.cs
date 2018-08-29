using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
namespace CloudCopyActivities
{
    class AWSHelper
    {
        // Amazon s3 client.
        private static AmazonS3Client s3Client;

        public static async Task FileDownloadFromS3(string strDestinationFileFullPath,string strBucketName, string strKeyName, string strAwsAccessKeyId, string strAwsSecretAccessKey, string strRegion)
        {
            RegionEndpoint region = getAWSRegion(strRegion);
            // Create Amazon S3 client 
            s3Client = new AmazonS3Client(strAwsAccessKeyId, strAwsSecretAccessKey, region);
                         
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = strBucketName,
                    Key = strKeyName
                };
                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))             
                response.WriteResponseStreamToFile(strDestinationFileFullPath);               
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }//end function FileDownloadFromS3

        public static async Task FileUploadToS3(string strBucketName, string strKeyName, string strSourceFilePath, string strAwsAccessKeyId, string strAwsSecretAccessKey, string strRegion)
        {

            // Create Amazon S3 client    
            RegionEndpoint region = getAWSRegion(strRegion);
            try
            {               
                s3Client = new AmazonS3Client(strAwsAccessKeyId, strAwsSecretAccessKey, region);
              
                var putRequest = new PutObjectRequest
                {                    
                    BucketName = strBucketName,
                    Key = strKeyName,                
                    FilePath = strSourceFilePath
                };

                PutObjectResponse response = await s3Client.PutObjectAsync(putRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object"
                    , e.Message);
            }
        }
        
        public static RegionEndpoint getAWSRegion(string strRegionName)
        {
            if (strRegionName == null) {
                return RegionEndpoint.USEast1;
            }
            strRegionName = strRegionName.ToLower();
            switch (strRegionName)
            {
                case "apnortheast1": return RegionEndpoint.APNortheast1;
                case "apnortheast2": return RegionEndpoint.APNortheast2;
                case "apsouth1": return RegionEndpoint.APSouth1;
                case "apsouthest1": return RegionEndpoint.APSoutheast1;
                case "apsouthest2": return RegionEndpoint.APSoutheast2;
                case "cacentral1": return RegionEndpoint.CACentral1;
                case "cnnorth1": return RegionEndpoint.CNNorth1;
                case "cnnorthwest1": return RegionEndpoint.CNNorthWest1;
                case "eucentral1": return RegionEndpoint.EUCentral1;
                case "euwest1": return RegionEndpoint.EUWest1;
                case "euwest2": return RegionEndpoint.EUWest2;
                case "euwest3": return RegionEndpoint.EUWest3;
                case "saeast1": return RegionEndpoint.SAEast1;
                case "useast1": return RegionEndpoint.USEast1;
                case "useast2": return RegionEndpoint.USEast2;
                case "usgovcloudwest1": return RegionEndpoint.USGovCloudWest1;
               
                default: return RegionEndpoint.USEast1;
            }            
        }

        }//end class
}//end namespace

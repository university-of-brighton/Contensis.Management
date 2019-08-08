using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zengenti.Contensis.Management;
using Zengenti.Contensis.Management.Workflow.Basic;
using Zengenti.Data;

namespace UniversityOfBrighton.Contensis.Management
{
    /// <summary>
    /// Helper class to make interacting with the Contensis Mangament API
    /// easier to manage content type entries using basic workflow 
    /// </summary>
    public class BasicCmsEntryManager
    {
        protected Project CmsProject;

        /// <summary>
        /// Use credentials to create manager 
        /// </summary>
        /// <param name="credentials"></param>
        public BasicCmsEntryManager(CmsCredentials credentials)
        {
            CmsProject = GetCmsProject(credentials);
        }

        /// <summary>
        /// Use an existing project to create manager
        /// </summary>
        /// <param name="cmsProject"></param>
        public BasicCmsEntryManager(Project cmsProject) => CmsProject = cmsProject;

        /// <summary>
        /// Create a Managemnet Client and fetch project using Credentials 
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public Project GetCmsProject(CmsCredentials credentials)
        {
            var client = ManagementClient.Create(
                credentials.RootUrl,
                credentials.ClientId,
                credentials.SharedSecret
            );
            return client.Projects.Get(credentials.ProjectName);
        }

        /// <summary>
        /// Fetch all the entries of a given content type form project
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="pageSize"></param>
        /// <param name="language"></param>
        /// <param name="maxNumber">If greater than 0 then limit the number of entries returned</param>
        /// <returns></returns>
        public IEnumerable<Entry> GetAllEntries(string contentTypeId, int pageSize = 25, string language = null, int maxNumber = 0)
        {
            int pageIndex = 0;
            int entryCount = 1;
            PagedList<Entry> entries = CmsProject.Entries.List(contentTypeId, language, new PageOptions(pageIndex, pageSize));

            while (entries.Items.Count > 0)
            {
                foreach (Entry entry in entries.Items)
                {
                    yield return entry;
                    entryCount++;
                    if ( maxNumber != 0 && entryCount > maxNumber)
                    {
                        yield break;
                    }
                }
                pageIndex++;
                entries = CmsProject.Entries.List(contentTypeId, language, new PageOptions(pageIndex, pageSize));
            }
        }

        /// <summary>
        /// Will Catch Errors from Entry.SaveAsync() and entry.Workflow.PublishAsync()
        /// and retry to publish up to maxRetries times
        /// </summary>
        /// <param name="entry">The entry to Publish</param>
        /// <param name="attempts">Count of attempts (default 0)<</param>
        /// <param name="maxRetries">Number of time to retry (default 10)</param>
        /// <returns></returns>
        public async Task<bool> PublishEntryAsync(Entry entry, int attempts = 0, int maxRetries = 10)
        {
            try
            {
                await entry.SaveAsync();
                await entry.Workflow.PublishAsync();
                return true;
            }
            catch (Exception)
            {
                if (attempts < maxRetries)
                {
                    return await PublishEntryAsync(entry, ++attempts, maxRetries);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Will Catch Errors from Entry.DeleteAsync and retry to Delete up to maxRetries times
        /// </summary>
        /// <param name="entry">The entry to Publish</param>
        /// <param name="attempts">Count of attempts (default 0)<</param>
        /// <param name="maxRetries">Number of time to retry (default 10)</param>
        /// <returns></returns>
        public async Task<bool> DeleteEntryAsync(Entry entry, int attempts = 0, int maxRetries = 10)
        {
            try
            {
                await entry.DeleteAsync();
                return true;
            }
            catch (Exception)
            {
                if (attempts < maxRetries)
                {
                    return await DeleteEntryAsync(entry, ++attempts, maxRetries);
                }
                else
                {
                    return false;
                }
            }
        }


    }
}

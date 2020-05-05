﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using DataModel;
using TfsData;
using ReleaseNotesService;

namespace ReleaseNotesGenerator.CLI
{
    class Program
    {
        private static Generator _generator;
        private static TfsConnector _tfs;
        static void Main(string[] args)
        {
            const string tfsProject = "";
            const string branch = "";
            const string changesetTo = "";

            const string qaBuildName = "";
            const string qaBuildDate = "";

            
            const string changesetFrom = "";
            const string releaseName = "";
            const string testReport = "";
            var iteration = $"Project\\Current\\{releaseName}";

            var releaseDate = DateTime.Now.ToString("d-MMMM-yyyy");//"14.02.2020";

            var red = new ReleaseData
            {
                TfsProject = tfsProject,
                TfsBranch = branch,
                ChangesetTo = changesetTo,
                QaBuildName = qaBuildName,
                QaBuildDate = qaBuildDate,
                ChangesetFrom = changesetFrom,
                IterationSelected = iteration,
                ReleaseDate = releaseDate,
                ReleaseName = releaseName
            };

            var tfsUrl = ConfigurationManager.AppSettings["tfsUrl"];
            var tfsUsername = ConfigurationManager.AppSettings["tfsUsername"];
            var tfsKey = ConfigurationManager.AppSettings["tfsKey"];
            var adoUrl = ConfigurationManager.AppSettings["adoUrl"];
            var adoUsername = ConfigurationManager.AppSettings["adoUsername"];
            var adoKey = ConfigurationManager.AppSettings["adoKey"];
            var documentLocation = ConfigurationManager.AppSettings["documentLocation"];
            if (string.IsNullOrWhiteSpace(tfsUrl)) return;

            _tfs = new TfsConnector(tfsUrl, tfsUsername, tfsKey, adoUrl, adoUsername, adoKey);
            _generator = new Generator(_tfs);

            var downloadedData = _generator.DownloadData(tfsProject, branch, changesetFrom, changesetTo, iteration).Result;

            var psRefresh = downloadedData.Changes.First(x => changesetTo == x.changesetId.ToString());

            var workItemStateInclude = GettrimmedSettingList("workItemStateInclude");

            var message = _generator.CreateDoc(downloadedData, psRefresh, workItemStateInclude, red, documentLocation, testReport);

            if (!string.IsNullOrWhiteSpace(message)) Console.WriteLine(message);
        }
        private static List<string> GettrimmedSettingList(string key)
        {
            return ConfigurationManager.AppSettings[key].Split(',').Select(x => x.Trim()).ToList();
        }
    }
}

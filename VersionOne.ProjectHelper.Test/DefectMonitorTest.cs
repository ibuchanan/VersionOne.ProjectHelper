using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VersionOne.SDK.APIClient;
using VersionOne.SDK.ObjectModel;

namespace VersionOne.ProjectHelper.Test
{
    [TestClass]
    public class DefectMonitorTest
    {
        [TestMethod]
        public void TestGetMostRecentChangedDateTime()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project called "Target Project 1" under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var targetProject = instance.Create.Project(Name("Target Project"), rootProject, DateTime.Now, schedule);
            // And a new defect under "Target Project"
            var newDefect = instance.Create.Defect(Name("New Defect"), targetProject);
            // And a new project helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // And a new defect monitor
            var monitor = new DefectMonitor(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the parent project is registered with the monitor
            monitor.MonitoredProjects.Add(projectList[targetProject.ID.Token]);
            // When I check to see the most recent change date
            var actual = monitor.GetMostRecentChangeDateTime();
            // Then the monitor tells me the date from the new defect.
            var expected = newDefect.ChangeDate.ToUniversalTime();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestHasChangedSinceWithNoChange()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project called "Target Project 1" under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var targetProject = instance.Create.Project(Name("Target Project"), rootProject, DateTime.Now, schedule);
            // And a defect under "Target Project"
            var newDefect = instance.Create.Defect(Name("New Defect"), targetProject);
            // And a new project helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // And a new defect monitor
            var monitor = new DefectMonitor(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the parent project is registered with the monitor
            monitor.MonitoredProjects.Add(projectList[targetProject.ID.Token]);
            // And the most recent change date is for the defect
            var lastChange = monitor.GetMostRecentChangeDateTime();
            // When check whether the system has changed since the remembered change date
            var actual = monitor.HasChangedSince(lastChange);
            // Then the monitor tells me there is no change
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void TestHasChangedSinceWithAChange()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project called "Target Project 1" under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var targetProject = instance.Create.Project(Name("Target Project"), rootProject, DateTime.Now, schedule);
            // And a defect under "Target Project"
            var newDefect = instance.Create.Defect(Name("New Defect"), targetProject);
            // And a new project helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // And a new defect monitor
            var monitor = new DefectMonitor(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the parent project is registered with the monitor
            monitor.MonitoredProjects.Add(projectList[targetProject.ID.Token]);
            // And the most recent change date is for the defect
            var lastChange = monitor.GetMostRecentChangeDateTime();
            // When I create a new defect
            var newerDefect = instance.Create.Defect(Name("Newer Defect"), targetProject);
            // And check whether the system has changed since the remembered change date
            var actual = monitor.HasChangedSince(lastChange);
            // Then the monitor tells me there is a change
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TestGetMostRecentNewDateTime()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project called "Target Project 1" under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var targetProject = instance.Create.Project(Name("Target Project"), rootProject, DateTime.Now, schedule);
            // And a new defect under "Target Project"
            var newDefect = instance.Create.Defect(Name("New Defect"), targetProject);
            // And a new project helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // And a new defect monitor
            var monitor = new DefectMonitor(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the parent project is registered with the monitor
            monitor.MonitoredProjects.Add(projectList[targetProject.ID.Token]);
            // When I check to see the most recent new date
            var actual = monitor.GetMostRecentNewDateTime();
            // Then the monitor tells me the date from the new defect.
            var expected = newDefect.CreateDate;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestHasNewSinceWithNoChange()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project called "Target Project 1" under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var targetProject = instance.Create.Project(Name("Target Project"), rootProject, DateTime.Now, schedule);
            // And a defect under "Target Project"
            var newDefect = instance.Create.Defect(Name("New Defect"), targetProject);
            // And a new project helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // And a new defect monitor
            var monitor = new DefectMonitor(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the parent project is registered with the monitor
            monitor.MonitoredProjects.Add(projectList[targetProject.ID.Token]);
            // And the most recent new date is for the defect
            var lastNew = monitor.GetMostRecentNewDateTime();
            // When I check whether the system has changed since the remembered new date
            var actual = monitor.HasNewSince(lastNew);
            // Then the monitor tells me there is nothing new
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void TestHasNewSinceWithAChange()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project called "Target Project 1" under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var targetProject = instance.Create.Project(Name("Target Project"), rootProject, DateTime.Now, schedule);
            // And a defect under "Target Project"
            var newDefect = instance.Create.Defect(Name("New Defect"), targetProject);
            // And a new project helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // And a new defect monitor
            var monitor = new DefectMonitor(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the parent project is registered with the monitor
            monitor.MonitoredProjects.Add(projectList[targetProject.ID.Token]);
            // And the most recent new date is for the defect
            var lastNew = monitor.GetMostRecentNewDateTime();
            // When I create a new defect
            var newerDefect = instance.Create.Defect(Name("Newer Defect"), targetProject);
            // And check whether the system has new since the remembered change date
            var actual = monitor.HasChangedSince(lastNew);
            // Then the monitor tells me there is a new defect
            Assert.IsTrue(actual);
        }

        private static string Name(string prefix)
        {
            return (prefix + " SystemAllProjects Helper " + DateTime.Now);
        }

    }
}

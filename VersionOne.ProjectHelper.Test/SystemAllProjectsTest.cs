using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VersionOne.SDK.APIClient;
using VersionOne.SDK.ObjectModel;

namespace VersionOne.ProjectHelper.Test
{
    [TestClass]
    public class SystemAllProjectsTest
    {
        [TestMethod]
        public void TestGetProjectPath()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var parentProject = instance.Create.Project(Name("Parent Project"), rootProject, DateTime.Now, schedule);
            // And a new child project under the parent with the same schedule
            var childProject = instance.Create.Project(Name("Child Project"), parentProject, DateTime.Now, schedule);
            // And a new helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // When I ask for the project path for the child project
            var actual = projectList[childProject.ID.Token].PathName;
            // Then I get a concatenation of the root project, the parent project, and the child project, in that order
            var expected = rootProject.Name + "\\" + parentProject.Name + "\\" + childProject.Name;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetProjectPathSixDeep()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var parentProject = instance.Create.Project(Name("Parent Project"), rootProject, DateTime.Now, schedule);
            // And a new child project under the parent with the same schedule
            var child3Project = instance.Create.Project(Name("Child Project"), parentProject, DateTime.Now, schedule);
            // And a new subchild project
            var child4Project = instance.Create.Project(Name("Child Project"), child3Project, DateTime.Now, schedule);
            // And a new subchild project
            var child5Project = instance.Create.Project(Name("Child Project"), child4Project, DateTime.Now, schedule);
            // And a new subchild project
            var child6Project = instance.Create.Project(Name("Child Project"), child5Project, DateTime.Now, schedule);
            // And a new helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // When I ask for the project path for the child project
            var actual = projectList[child6Project.ID.Token].PathName;
            // Then I get a concatenation of the root project, the parent project, and the child project, in that order
            var expected = rootProject.Name + "\\" + parentProject.Name + "\\" + child3Project.Name + "\\" + child4Project.Name + "\\" + child5Project.Name + "\\" + child6Project.Name;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetProjectWhereParentAndGrandchildAreInverted()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var parentProject = instance.Create.Project(Name("Parent Project"), rootProject, DateTime.Now, schedule);
            // And a new child project under the parent with the same schedule
            var childProject = instance.Create.Project(Name("Child Project"), parentProject, DateTime.Now, schedule);
            // And a new grandchild project under the root
            var grandChildProject = instance.Create.Project(Name("Grandchild Project"), rootProject, DateTime.Now, schedule);
            // And move the Parent into the Grandchild
            parentProject.ParentProject = grandChildProject;
            parentProject.Save();
            // And a new helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // When I ask for the project path for the child project
            var actual = projectList[childProject.ID.Token].PathName;
            // Then I get a concatenation of the root project, the parent project, and the child project, in that order
            var expected = rootProject.Name + "\\" + grandChildProject.Name + "\\" + parentProject.Name + "\\" + childProject.Name;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultipleProjectPaths()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var parentProject = instance.Create.Project(Name("Parent Project"), rootProject, DateTime.Now, schedule);
            // And a new child project under the parent with the same schedule
            var childProject = instance.Create.Project(Name("Child Project"), parentProject, DateTime.Now, schedule);
            // And a new helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // When I ask for the project path for the parent project
            var parentPath = v1System.GetProjects()[parentProject.ID.Token];
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // And the project path for the child project
            var actual = projectList[childProject.ID.Token].PathName;
            // Then I get a concatenation of the root project, the parent project, and the child project, in that order
            var expected = rootProject.Name + "\\" + parentProject.Name + "\\" + childProject.Name;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNewHelperIsDirty()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // When I create a new Helper
            var helper = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // Then it is initially dirty
            Assert.IsTrue(helper.IsDirty());
        }

        [TestMethod]
        public void TestReloadIsClean()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // When I create a new Helper
            var helper = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And reload the project data
            helper.Reload();
            // Then the helper is not dirty
            Assert.IsFalse(helper.IsDirty());
        }

        [TestMethod]
        public void TestRenameIsDirty()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new project under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var project = instance.Create.Project(Name("Project"), rootProject, DateTime.Now, schedule);
            // And a new helper
            var helper = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the helper is clean after a reload
            helper.Reload();
            // When I change the project name
            project.Name = Name("New Name");
            project.Save();
            // Then the helper is dirty
            Assert.IsTrue(helper.IsDirty());
        }

        [TestMethod]
        public void TestMoveIsDirty()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var parentProject = instance.Create.Project(Name("Parent Project"), rootProject, DateTime.Now, schedule);
            // And a new child project under the parent with the same schedule
            var childProject = instance.Create.Project(Name("Child Project"), parentProject, DateTime.Now, schedule);
            // And a new helper
            var helper = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the helper is clean after a reload
            helper.Reload();
            // When I move the child project
            childProject.ParentProject = rootProject;
            childProject.Save();
            // Then the helper is dirty
            Assert.IsTrue(helper.IsDirty());
        }

        [TestMethod]
        public void TestChildrenMeAndDown()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var parentProject = instance.Create.Project(Name("Parent Project"), rootProject, DateTime.Now, schedule);
            // And a new child project under the parent with the same schedule
            var child3Project = instance.Create.Project(Name("Child Project"), parentProject, DateTime.Now, schedule);
            // And a new subchild project
            var child4Project = instance.Create.Project(Name("Child Project"), child3Project, DateTime.Now, schedule);
            // And a new subchild project
            var child5Project = instance.Create.Project(Name("Child Project"), child4Project, DateTime.Now, schedule);
            // And a new subchild project
            var child6Project = instance.Create.Project(Name("Child Project"), child5Project, DateTime.Now, schedule);
            // And a new helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // When I ask for the count of children, me, and down from the parent project
            var actual = projectList[parentProject.ID.Token].ChildrenMeAndDown.Count;
            // Then I get 5.
            var expected = 5;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestConstructionOfProjectList()
        {
            // Given a connection to an instance of VersionOne at https://www14.v1host.com/v1sdktesting/ with user credentials for admin
            var instance = new V1Instance("https://www14.v1host.com/v1sdktesting/", "admin", "admin");
            // And a new schedule with 7 day length and no gap
            var schedule = instance.Create.Schedule(Name("Schedule"), Duration.Parse(@"7 Days"), Duration.Parse(@"0 Days"));
            // And a new parent project called "Target Project 1" under the root with that schedule
            var rootProject = instance.Get.ProjectByID(AssetID.FromToken("Scope:0"));
            var target1Project = instance.Create.Project(Name("Target Project 1"), rootProject, DateTime.Now, schedule);
            // And a new child project under "Target Project 1" with the same schedule
            var targetChildProject = instance.Create.Project(Name("Child Target Project"), target1Project, DateTime.Now, schedule);
            // And another project under the root called "Target Project 2"
            var target2Project = instance.Create.Project(Name("Target Project 2"), rootProject, DateTime.Now, schedule);
            // And a new child project under "Target Project 2" with the same schedule called "Child Non-target Project"
            var nontargetChildProject = instance.Create.Project(Name("Child Non-target Project"), target2Project, DateTime.Now, schedule);
            // And a new helper
            var v1System = new SystemAllProjects(instance.ApiClient.Services, instance.ApiClient.MetaModel);
            // And the list of all projects
            var projectList = v1System.GetProjects();
            // When I ask for the list of projects for "Target Project 1" with descendants and "Target Project 2" without descendants
            var myProjectList = new List<Project>();
            myProjectList.AddRange(projectList[target1Project.ID.Token].ChildrenMeAndDown);
            myProjectList.AddRange(projectList[target2Project.ID.Token].JustMe);
            // Then I don't get "Child Non-target Project" in the results.
            Assert.IsFalse(myProjectList.Contains(projectList[nontargetChildProject.ID.Token]));
        }

        private static string Name(string prefix)
        {
            return (prefix + " SystemAllProjects Helper " + DateTime.Now);
        }

    }
}

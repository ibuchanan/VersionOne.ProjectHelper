using System.Collections;
using System.Collections.Generic;
using VersionOne.SDK.ObjectModel;
using VersionOne.SDK.ObjectModel.Filters;

namespace VersionOne.ProjectHelper
{
    public class Project : IEnumerable
    {
        public Project(string scopeId)
        {
            ScopeId = scopeId;
            Children = new List<Project>();
            _justMe = new List<Project> { this };
        }

        private readonly IList<Project> _justMe;
        public string ScopeId { get; set; }
        public string Name { get; set; }
        public string PathName { get; set; }
        public string ParentScopeId { get; set; }
        public IList<Project> Children { get; set; }
        public IList<Project> ChildrenMeAndDown
        {
            get
            {
                var descendants = new List<Project> { this };
                foreach (var child in Children)
                {
                    descendants.AddRange(child.ChildrenMeAndDown);
                }
                return descendants;
            }
        }
        public IList<Project> JustMe
        {
            get
            {
                return _justMe;
            }
        }

        public void AddToDefectFilter(V1Instance v1Instance, DefectFilter filter, bool includeDecendants)
        {
            var projectList = includeDecendants ? ChildrenMeAndDown : JustMe;
            foreach (var project in projectList)
            {
                filter.Project.Add(v1Instance.Get.ProjectByID(project.ScopeId));
            }
        }

        //Implemention of IEnumerable
        public IEnumerator GetEnumerator()
        {
            return Children.GetEnumerator();
        }

    }
}
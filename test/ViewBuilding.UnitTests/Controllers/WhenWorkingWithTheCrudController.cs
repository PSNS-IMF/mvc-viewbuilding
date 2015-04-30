using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

using Psns.Common.Test.BehaviorDrivenDevelopment;
using Psns.Common.Mvc.ViewBuilding.Controllers;
using Psns.Common.Mvc.ViewBuilding.ViewBuilders;
using Psns.Common.Mvc.ViewBuilding.ViewModels;
using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;
using Psns.Common.Persistence.Definitions;
using Psns.Common.Web.Adapters;

using System.Web.Mvc;

using Moq;

namespace ViewBuilding.UnitTests.Controllers.Crud
{
    public class TestEntitiesController : CrudController<TestEntity>
    {
        public TestEntitiesController(ICrudViewBuilder viewBuilder, IRepositoryFactory factory)
            : base(viewBuilder, factory) { }
    }

    public class WhenWorkingWithTheCrudController : BehaviorDrivenDevelopmentCaseTemplate
    {
        protected TestEntitiesController Controller;
        protected Mock<ICrudViewBuilder> MockViewBuilder;
        protected Mock<IRepositoryFactory> MockFactory;
        protected Mock<IRepository<TestEntity>> MockRepository;

        public override void Arrange()
        {
            base.Arrange();

            MockViewBuilder = new Mock<ICrudViewBuilder>();
            MockFactory = new Mock<IRepositoryFactory>();
            MockRepository = new Mock<IRepository<TestEntity>>();

            MockFactory.Setup(f => f.Get<TestEntity>()).Returns(MockRepository.Object);

            Controller = new TestEntitiesController(MockViewBuilder.Object, MockFactory.Object);
            ControllerTestHelper.SetContext(Controller);
        }
    }

    [TestClass]
    public class AndCheckingForRequireRequestAttributes : BehaviorDrivenDevelopmentCaseTemplate
    {
        List<string> _requiredValues;

        public override void Arrange()
        {
            base.Arrange();

            _requiredValues = new List<string>();
        }

        public override void Act()
        {
            base.Act();

            new[] { "Details", "Delete" }.ToList().ForEach(name =>
            {
                var methodInfo = typeof(CrudController<>).GetMember(name)
                    .Where(m => m.GetCustomAttributes(typeof(RequireRequestValueAttribute), true).Any()).First();

                _requiredValues.Add((methodInfo.GetCustomAttributes(typeof(RequireRequestValueAttribute), false)[0] 
                    as RequireRequestValueAttribute).ValueName);
            });
        }

        [TestMethod]
        public void ThenTheDetailsAndDeleteMethodsShouldBeProperlyDecorated()
        {
            foreach(var name in _requiredValues)
                Assert.AreEqual("model", name);
        }
    }

    #region Index

    [TestClass]
    public class AndCallingIndex : WhenWorkingWithTheCrudController
    {
        ViewResult _result;

        public override void Arrange()
        {
            base.Arrange();

            MockViewBuilder.Setup(b => b.BuildIndexView<TestEntity>(
                It.IsAny<int?>(), 
                It.IsAny<int?>(), 
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<IIndexViewVisitor[]>()
            ))
                .Returns(new IndexView(typeof(TestEntity).Name));
        }

        public override void Act()
        {
            base.Act();

            _result = Controller.Index() as ViewResult;
        }

        [TestMethod]
        public void ThenAViewResultShouldBeReturnedWithTheIndexViewModel()
        {
            Assert.IsInstanceOfType(_result.Model, typeof(IndexView));
        }
    }

    #endregion


    #region Details

    [TestClass]
    public class AndCallingDetailsWithId : WhenWorkingWithTheCrudController
    {
        ViewResult _result;

        public override void Arrange()
        {
            base.Arrange();

            MockViewBuilder.Setup(b => b.BuildDetailsView<TestEntity>(1)).Returns(new DetailsView());
        }

        public override void Act()
        {
            base.Act();

            _result = Controller.Details(1) as ViewResult;
        }

        [TestMethod]
        public void ThenAViewResultShouldBeReturnedWithTheDetailsViewModel()
        {
            Assert.IsInstanceOfType(_result.Model, typeof(DetailsView));
            MockViewBuilder.Verify(b => b.BuildDetailsView<TestEntity>(1, It.IsAny<IDetailsViewVisitor[]>()), Times.Once());
        }
    }

    [TestClass]
    public class AndCallingDetailsWithModel : WhenWorkingWithTheCrudController
    {
        ViewResult _result;
        TestEntity _model;

        public override void Arrange()
        {
            base.Arrange();

            _model = new TestEntity { Id = 1 };
            MockViewBuilder.Setup(b => b.BuildDetailsView<TestEntity>(_model)).Returns(new DetailsView());
        }

        public override void Act()
        {
            base.Act();

            _result = Controller.Details(_model) as ViewResult;
        }

        [TestMethod]
        public void ThenAViewResultShouldBeReturnedWithTheDetailsViewModel()
        {
            Assert.IsInstanceOfType(_result.Model, typeof(DetailsView));
            MockViewBuilder.Verify(b => b.BuildDetailsView<TestEntity>(_model, It.IsAny<IDetailsViewVisitor[]>()), Times.Once());
        }
    }

    #endregion


    #region Update

    [TestClass]
    public class AndCallingUpdateGet : WhenWorkingWithTheCrudController
    {
        ViewResult _result;

        public override void Arrange()
        {
            base.Arrange();

            MockViewBuilder.Setup(b => b.BuildUpdateView<TestEntity>(1)).Returns(new UpdateView());
        }

        public override void Act()
        {
            base.Act();

            _result = Controller.Update(1) as ViewResult;
        }

        [TestMethod]
        public void ThenAViewResultShouldBeReturnedWithTheUpdateViewModel()
        {
            Assert.IsInstanceOfType(_result.Model, typeof(UpdateView));
        }
    }

    public class AndCallingUpdateModel : WhenWorkingWithTheCrudController
    {
        protected TestEntity Entity;
        protected List<ActionResult> Results;

        public override void Arrange()
        {
            base.Arrange();

            Entity = new TestEntity { Id = 1 };

            Results = new List<ActionResult>();

            MockViewBuilder.Setup(b => b.BuildUpdateView<TestEntity>(Entity)).Returns(new UpdateView());
            MockRepository.Setup(r => r.Update(It.IsAny<TestEntity>(),
                "AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey")).Returns(Entity);
        }
    }

    public class AndCallingUpdateModelPost : AndCallingUpdateModel
    {
        protected int TimesValidateCalled;

        public override void Arrange()
        {
            base.Arrange();

            AntiForgeryHelperAdapter.ValidationFunction = () => TimesValidateCalled++;
        }

        public override void Act()
        {
            base.Act();

            new[] { HttpVerbs.Post, HttpVerbs.Put, HttpVerbs.Patch, HttpVerbs.Delete }.ToList().ForEach(verb =>
            {
                Results.Add(Controller.Update(Entity, verb));
            });
        }

        protected void AssertCommon()
        {
            Assert.AreEqual(4, TimesValidateCalled);
        }
    }

    [TestClass]
    public class WhenTheModelStateIsValid : AndCallingUpdateModelPost
    {
        [TestMethod]
        public void ThenTheRepositoryShouldBeUpdatedAndARedirectToDetailsShouldBeReturned()
        {
            foreach(var actionResult in Results)
            {
                var result = actionResult as RedirectToRouteResult;
                Assert.AreEqual("Details", result.RouteName);
                Assert.AreEqual("Details", result.RouteValues["action"]);
                Assert.AreEqual(1, result.RouteValues["id"]);
            }

            MockRepository.Verify(r => r.Update(It.Is<TestEntity>(e => e.Id == 1), 
                "AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey"), Times.Exactly(4));
            MockRepository.Verify(r => r.SaveChanges(), Times.Exactly(4));
        }
    }

    [TestClass]
    public class WhenTheModelStateIsInValid : AndCallingUpdateModelPost
    {
        public override void Arrange()
        {
            base.Arrange();

            MockViewBuilder.Setup(b => b.BuildUpdateView<TestEntity>(It.IsAny<TestEntity>())).Returns(new UpdateView());

            Controller.ModelState.AddModelError("Id", "Id is not here");
        }

        [TestMethod]
        public void ThenAViewResultWithTheBadModelShouldBeReturned()
        {
            foreach(var actionResult in Results)
            {
                var result = actionResult as ViewResult;
                Assert.IsInstanceOfType(result.Model, typeof(UpdateView));
            }

            MockRepository.Verify(r => r.Update(It.Is<TestEntity>(e => e.Id == 1)), Times.Never());
        }
    }

    [TestClass]
    public class WhenTheIdEqualsZero : AndCallingUpdateModelPost
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.Create(It.IsAny<TestEntity>())).Returns(new TestEntity { Id = 1 });
        }

        public override void Act()
        {
            new[] { HttpVerbs.Post, HttpVerbs.Put, HttpVerbs.Patch, HttpVerbs.Delete }.ToList().ForEach(verb =>
            {
                Results.Add(Controller.Update(new TestEntity { Id = 0 }, verb));
            });
        }

        [TestMethod]
        public void ThenAViewResultWithTheModelShouldBeReturnedAndRepoCreateShouldBeCalled()
        {
            foreach(var actionResult in Results)
            {
                var result = actionResult as RedirectToRouteResult;
                Assert.AreEqual("Details", result.RouteName);
                Assert.AreEqual("Details", result.RouteValues["action"]);
                Assert.AreEqual(1, result.RouteValues["id"]);
            }

            MockRepository.Verify(r => r.Create(It.Is<TestEntity>(e => e.Id == 0)), Times.Exactly(4));
            MockRepository.Verify(r => r.SaveChanges(), Times.Exactly(4));
        }
    }

    [TestClass]
    public class AndCallingUpdateModelNonPosts : AndCallingUpdateModel
    {
        public override void Act()
        {
            base.Act();

            new[] { HttpVerbs.Get, HttpVerbs.Head, HttpVerbs.Options }.ToList().ForEach(verb =>
            {
                Results.Add(Controller.Update(Entity, verb));
            });
        }

        [TestMethod]
        public void ThenAViewResultShouldBeReturnedWithTheUpdateViewModel()
        {
            foreach(var actionResult in Results)
            {
                var result = actionResult as ViewResult;
                Assert.IsInstanceOfType(result.Model, typeof(UpdateView));
            }

            MockViewBuilder.Verify(b => b.BuildUpdateView<TestEntity>(Entity), Times.Exactly(3));

            MockRepository.Verify(r => r.Create(It.IsAny<TestEntity>()), Times.Never());
            MockRepository.Verify(r => r.Update(It.IsAny<TestEntity>(), It.IsAny<string[]>()), Times.Never());
            MockRepository.Verify(r => r.SaveChanges(), Times.Never());
        }
    }

    #endregion

    #region Delete

    public class AndDeleting : WhenWorkingWithTheCrudController
    {
        protected TestEntity BeingTested;
        protected bool ValidateCalled;

        public override void Arrange()
        {
            base.Arrange();

            BeingTested = new TestEntity { Id = 1 };
            MockRepository.Setup(r => r.Find(1)).Returns(BeingTested);

            AntiForgeryHelperAdapter.ValidationFunction = () => ValidateCalled = true;
        }

        protected void AssertCommon()
        {
            Assert.IsTrue(ValidateCalled);
            MockRepository.Verify(r => r.Delete(It.Is<TestEntity>(e => e.Id == BeingTested.Id)), Times.Once());
            MockRepository.Verify(r => r.SaveChanges(), Times.Once());
        }
    }

    [TestClass]
    public class AndDeletingById : AndDeleting
    {
        public override void Act()
        {
            base.Act();

           Controller.Delete(1);
        }

        [TestMethod]
        public void ThenTheRepositoryDeleteShouldBeCalledWithTheRightObject()
        {
            AssertCommon();
        }
    }

    [TestClass]
    public class AndDeletingByModel : AndDeleting
    {
        public override void Act()
        {
            base.Act();

            Controller.Delete(BeingTested);
        }

        [TestMethod]
        public void ThenTheRepositoryDeleteShouldBeCalledWithTheRightObject()
        {
            AssertCommon();
        }
    }
    #endregion
}

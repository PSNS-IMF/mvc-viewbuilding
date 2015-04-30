using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    public class AndCallingUpdatePost : WhenWorkingWithTheCrudController
    {
        protected bool ValidateCalled;
        protected ActionResult Result;
        protected TestEntity Entity;

        public override void Arrange()
        {
            base.Arrange();

            Entity = new TestEntity { Id = 1 };

            MockViewBuilder.Setup(b => b.BuildUpdateView<TestEntity>(1)).Returns(new UpdateView());
            MockRepository.Setup(r => r.Update(It.IsAny<TestEntity>(), 
                "AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey")).Returns(Entity);

            AntiForgeryHelperAdapter.ValidationFunction = () => ValidateCalled = true;
        }

        public override void Act()
        {
            base.Act();

            Result = Controller.Update(Entity);
        }

        protected void AssertCommon()
        {
            Assert.IsTrue(ValidateCalled);
        }
    }

    [TestClass]
    public class WhenTheModelStateIsValid : AndCallingUpdatePost
    {
        [TestMethod]
        public void ThenTheRepositoryShouldBeUpdatedAndARedirectToDetailsShouldBeReturned()
        {
            var result = Result as RedirectToRouteResult;
            Assert.AreEqual("Details", result.RouteName);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(1, result.RouteValues["id"]);

            MockRepository.Verify(r => r.Update(It.Is<TestEntity>(e => e.Id == 1), 
                "AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey"), Times.Once());
            MockRepository.Verify(r => r.SaveChanges(), Times.Once());
        }
    }

    [TestClass]
    public class WhenTheModelStateIsInValid : AndCallingUpdatePost
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
            var result = Result as ViewResult;
            Assert.IsInstanceOfType(result.Model, typeof(UpdateView));

            MockRepository.Verify(r => r.Update(It.Is<TestEntity>(e => e.Id == 1)), Times.Never());
        }
    }

    [TestClass]
    public class WhenTheIdEqualsZero : AndCallingUpdatePost
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.Create(It.IsAny<TestEntity>())).Returns(new TestEntity { Id = 1 });
        }

        public override void Act()
        {
            Result = Controller.Update(new TestEntity { Id = 0 });
        }

        [TestMethod]
        public void ThenAViewResultWithTheModelShouldBeReturnedAndRepoCreateShouldBeCalled()
        {
            var result = Result as RedirectToRouteResult;
            Assert.AreEqual("Details", result.RouteName);
            Assert.AreEqual("Details", result.RouteValues["action"]);
            Assert.AreEqual(1, result.RouteValues["id"]);

            MockRepository.Verify(r => r.Create(It.Is<TestEntity>(e => e.Id == 0)), Times.Once());
            MockRepository.Verify(r => r.SaveChanges(), Times.Once());
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

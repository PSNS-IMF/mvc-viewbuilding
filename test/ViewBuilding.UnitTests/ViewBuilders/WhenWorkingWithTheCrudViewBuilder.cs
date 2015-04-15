using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Psns.Common.Test.BehaviorDrivenDevelopment;

using Psns.Common.Mvc.ViewBuilding.ViewModels;
using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;
using Psns.Common.Mvc.ViewBuilding.ViewBuilders;
using Psns.Common.Mvc.ViewBuilding.Adapters;
using Psns.Common.Mvc.ViewBuilding.Attributes;

using Psns.Common.Persistence.Definitions;

using Moq;

namespace ViewBuilding.UnitTests.ViewBuilders
{
    public class WhenWorkingWithTheCrudViewBuilder : BehaviorDrivenDevelopmentCaseTemplate
    {
        protected CrudViewBuilder Builder;
        protected Mock<IRepository<TestEntity>> MockRepository;
        protected Mock<IRepositoryFactory> MockRepositoryFactory;

        public override void Arrange()
        {
            base.Arrange();

            MockRepository = new Mock<IRepository<TestEntity>>();
            MockRepositoryFactory = new Mock<IRepositoryFactory>();
            MockRepositoryFactory.Setup(f => f.Get<TestEntity>()).Returns(MockRepository.Object);

            Builder = new CrudViewBuilder(MockRepositoryFactory.Object);
        }
    }

    #region IndexView

    public class AndBuildingTheIndexView : WhenWorkingWithTheCrudViewBuilder
    {
        protected IndexView IndexView;
        protected Mock<IIndexViewVisitor> MockViewVisitor;

        public override void Arrange()
        {
            base.Arrange();

            MockViewVisitor = new Mock<IIndexViewVisitor>();

            RouteTable.Routes.Clear();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var mockHttpRequest = new Mock<HttpRequestBase>();
            mockHttpRequest.Setup(r => r.Url).Returns(new Uri("https://localhost.com:443"));

            var mockHttpResponse = new Mock<HttpResponseBase>();
            mockHttpResponse.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(s => s);

            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext
                .Setup(ctx => ctx.Request)
                .Returns(mockHttpRequest.Object);
            mockHttpContext
                .Setup(ctx => ctx.Response)
                .Returns(mockHttpResponse.Object);

            RequestContextAdapter.Context = new RequestContext(mockHttpContext.Object, new RouteData());
        }

        protected virtual void AssertCommon()
        {
            Assert.AreEqual<string>("TestEntities", IndexView.Title);
            Assert.AreEqual(2, IndexView.Table.Rows.Count);
            Assert.AreEqual(typeof(TestEntity).AssemblyQualifiedName, IndexView.ModelName);

            Assert.AreEqual(8, IndexView.Table.Header.Columns.Count);
            Assert.AreEqual("Name", IndexView.Table.Header.Columns[0].Value);
            Assert.AreEqual("Id", IndexView.Table.Header.Columns[1].Value);
            Assert.AreEqual("Named Name", IndexView.Table.Header.Columns[2].Value);
            Assert.AreEqual("Another Entity", IndexView.Table.Header.Columns[3].Value);
            Assert.AreEqual("AnotherEntity", IndexView.Table.Header.Columns[4].Value);
            Assert.AreEqual("Other Another Entity", IndexView.Table.Header.Columns[5].Value);

            var action = IndexView.CreateButton;
            Assert.AreEqual(ActionType.Link, action.Type);
            Assert.AreEqual("Default", action.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Update }", action.RouteValues.ToString());
            
            Assert.AreEqual("{ class = pure-button button-success, title = Create New TestEntity }", 
                action.Html.ToString());
            Assert.AreEqual("fa fa-plus fa-lg", string.Join(" ", action.IconHtmlClasses));

            Assert.AreEqual("{ id = searchButton, class = pure-button pure-button-primary, data-link = /IndexView/RefreshTableBody }", IndexView.SearchControl.Button.Html.ToString());
            Assert.AreEqual("fa fa-search", string.Join(" ", IndexView.SearchControl.Button.IconHtmlClasses));
            Assert.AreEqual("{ class = pure-input, style = font-size: 100%;, name = searchQuery, type = text }", IndexView.SearchControl.InputHtml.ToString());

            MockViewVisitor.Verify(v => v.Visit(IndexView.Table), Times.Once());
            MockViewVisitor.Verify(v => v.Visit(IndexView), Times.Once());

            foreach(var row in IndexView.Table.Rows)
            {
                MockViewVisitor.Verify(v => v.Visit(row), Times.Once());
                Assert.IsNotNull(row.Source);

                foreach(var column in row.Columns)
                {
                    MockViewVisitor.Verify(v => v.Visit(column), Times.Once());
                    Assert.IsNotNull(column.Source);
                }
            }
        }
    }

    public class AndBuildingAPagedIndexView : AndBuildingTheIndexView
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.All("AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey")).Returns(() =>
            {
                var list = new List<TestEntity>();

                for(int i = 0; i < 500; i++)
                {
                    list.Add(new TestEntity { Id = i });
                }

                return list;
            });
        }

        protected override void AssertCommon()
        {
            Assert.AreEqual("{ class = pure-paginator context-element, style = position:absolute;right:0px; }", 
                IndexView.Pager.Html.ToString());
        }
    }

    [TestClass]
    public class AndBuildingAPagedIndexViewWhenOnPageOneAndHasMoreThanOnePage : AndBuildingAPagedIndexView
    {
        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(pageSize: 50);
        }

        [TestMethod]
        public void ThenTheIndexViewShouldHaveTheCorrectPagingButton()
        {
            AssertCommon();

            Assert.AreEqual(50, IndexView.Table.Rows.Count);

            var pager = IndexView.Pager;

            Assert.IsNull(pager.First);
            Assert.IsNull(pager.Previous);

            Assert.AreEqual(ActionType.Button, pager.PagerState.Type);
            Assert.AreEqual("{ class = pure-button pure-button-disabled }", pager.PagerState.Html.ToString());
            Assert.AreEqual("1 of 10", pager.PagerState.Text);

            Assert.AreEqual(ActionType.Button, pager.Next.Type);
            Assert.AreEqual("{ class = pure-button, title = Go to page 2, data-link = /IndexView/RefreshTableBody?page=2 }", pager.Next.Html.ToString());
            Assert.AreEqual("fa fa-angle-right fa-lg", string.Join(" ", pager.Next.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.Last.Type);
            Assert.AreEqual("{ class = pure-button next, title = Go to page 10, data-link = /IndexView/RefreshTableBody?page=10 }", pager.Last.Html.ToString());
            Assert.AreEqual("fa fa-angle-double-right fa-lg", string.Join(" ", pager.Last.IconHtmlClasses));
        }
    }

    [TestClass]
    public class AndBuildingAPagedIndexViewWithASearchQuery : AndBuildingAPagedIndexView
    {
        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(pageSize: 50, searchQuery: "1");
        }

        [TestMethod]
        public void ThenTheIndexViewShouldHaveTheCorrectPagingButton()
        {
            AssertCommon();

            Assert.AreEqual(50, IndexView.Table.Rows.Count);

            var pager = IndexView.Pager;

            Assert.IsNull(pager.First);
            Assert.IsNull(pager.Previous);

            Assert.AreEqual(ActionType.Button, pager.PagerState.Type);
            Assert.AreEqual("{ class = pure-button pure-button-disabled }", pager.PagerState.Html.ToString());
            Assert.AreEqual("1 of 10", pager.PagerState.Text);

            Assert.AreEqual(ActionType.Button, pager.Next.Type);
            Assert.AreEqual("{ class = pure-button, title = Go to page 2, data-link = /IndexView/RefreshTableBody?page=2&searchQuery=1 }", pager.Next.Html.ToString());
            Assert.AreEqual("fa fa-angle-right fa-lg", string.Join(" ", pager.Next.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.Last.Type);
            Assert.AreEqual("{ class = pure-button next, title = Go to page 10, data-link = /IndexView/RefreshTableBody?page=10&searchQuery=1 }", pager.Last.Html.ToString());
            Assert.AreEqual("fa fa-angle-double-right fa-lg", string.Join(" ", pager.Last.IconHtmlClasses));
        }
    }

    [TestClass]
    public class AndBuildingAPagedIndexViewWhenOnPageTwoAndHasMoreThanOnePage : AndBuildingAPagedIndexView
    {
        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(2, pageSize: 50);
        }

        [TestMethod]
        public void ThenTheIndexViewShouldHaveTheCorrectPagingButton()
        {
            AssertCommon();

            Assert.AreEqual(50, IndexView.Table.Rows.Count);

            var pager = IndexView.Pager;

            Assert.IsNull(pager.First);

            Assert.AreEqual(ActionType.Button, pager.Previous.Type);
            Assert.AreEqual("{ class = pure-button prev, title = Go to page 1, data-link = /IndexView/RefreshTableBody?page=1 }", pager.Previous.Html.ToString());
            Assert.AreEqual("fa fa-angle-left fa-lg", string.Join(" ", pager.Previous.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.PagerState.Type);
            Assert.AreEqual("{ class = pure-button pure-button-disabled }", pager.PagerState.Html.ToString());
            Assert.AreEqual("2 of 10", pager.PagerState.Text);

            Assert.AreEqual(ActionType.Button, pager.Next.Type);
            Assert.AreEqual("{ class = pure-button, title = Go to page 3, data-link = /IndexView/RefreshTableBody?page=3 }", pager.Next.Html.ToString());
            Assert.AreEqual("fa fa-angle-right fa-lg", string.Join(" ", pager.Next.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.Last.Type);
            Assert.AreEqual("{ class = pure-button next, title = Go to page 10, data-link = /IndexView/RefreshTableBody?page=10 }", pager.Last.Html.ToString());
            Assert.AreEqual("fa fa-angle-double-right fa-lg", string.Join(" ", pager.Last.IconHtmlClasses));
        }
    }

    [TestClass]
    public class AndBuildingAPagedIndexViewWhenOnPageThreeAndHasMoreThanOnePage : AndBuildingAPagedIndexView
    {
        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(3, pageSize: 50);
        }

        [TestMethod]
        public void ThenTheIndexViewShouldHaveTheCorrectPagingButton()
        {
            AssertCommon();

            Assert.AreEqual(50, IndexView.Table.Rows.Count);

            var pager = IndexView.Pager;

            Assert.AreEqual(ActionType.Button, pager.First.Type);
            Assert.AreEqual("{ class = pure-button prev, title = Go to page 1, data-link = /IndexView/RefreshTableBody?page=1 }", pager.First.Html.ToString());
            Assert.AreEqual("fa fa-angle-double-left fa-lg", string.Join(" ", pager.First.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.Previous.Type);
            Assert.AreEqual("{ class = pure-button, title = Go to page 2, data-link = /IndexView/RefreshTableBody?page=2 }", pager.Previous.Html.ToString());
            Assert.AreEqual("fa fa-angle-left fa-lg", string.Join(" ", pager.Previous.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.PagerState.Type);
            Assert.AreEqual("{ class = pure-button pure-button-disabled }", pager.PagerState.Html.ToString());
            Assert.AreEqual("3 of 10", pager.PagerState.Text);

            Assert.AreEqual(ActionType.Button, pager.Next.Type);
            Assert.AreEqual("{ class = pure-button, title = Go to page 4, data-link = /IndexView/RefreshTableBody?page=4 }", pager.Next.Html.ToString());
            Assert.AreEqual("fa fa-angle-right fa-lg", string.Join(" ", pager.Next.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.Last.Type);
            Assert.AreEqual("{ class = pure-button next, title = Go to page 10, data-link = /IndexView/RefreshTableBody?page=10 }", pager.Last.Html.ToString());
            Assert.AreEqual("fa fa-angle-double-right fa-lg", string.Join(" ", pager.Last.IconHtmlClasses));
        }
    }

    [TestClass]
    public class AndBuildingAPagedIndexAndOnTheLastPage : AndBuildingAPagedIndexView
    {
        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(10, pageSize: 50);
        }

        [TestMethod]
        public void ThenTheIndexViewShouldHaveTheCorrectPagingButton()
        {
            AssertCommon();

            Assert.AreEqual(50, IndexView.Table.Rows.Count);

            var pager = IndexView.Pager;

            Assert.AreEqual(ActionType.Button, pager.First.Type);
            Assert.AreEqual("{ class = pure-button prev, title = Go to page 1, data-link = /IndexView/RefreshTableBody?page=1 }", pager.First.Html.ToString());
            Assert.AreEqual("fa fa-angle-double-left fa-lg", string.Join(" ", pager.First.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.Previous.Type);
            Assert.AreEqual("{ class = pure-button, title = Go to page 9, data-link = /IndexView/RefreshTableBody?page=9 }", pager.Previous.Html.ToString());
            Assert.AreEqual("fa fa-angle-left fa-lg", string.Join(" ", pager.Previous.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.PagerState.Type);
            Assert.AreEqual("{ class = pure-button pure-button-disabled }", pager.PagerState.Html.ToString());
            Assert.AreEqual("10 of 10", pager.PagerState.Text);

            Assert.IsNull(pager.Next);
            Assert.IsNull(pager.Last);
        }
    }

    [TestClass]
    public class AndBuildingAPagedIndexThatIsSorted : AndBuildingAPagedIndexView
    {
        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(8, pageSize: 50, sortKey: "id", sortDirection: "desc");
        }

        [TestMethod]
        public void ThenTheIndexViewShouldHaveTheCorrectPagingButton()
        {
            AssertCommon();

            Assert.AreEqual(50, IndexView.Table.Rows.Count);

            var pager = IndexView.Pager;
            Assert.AreEqual(ActionType.Button, pager.First.Type);
            Assert.AreEqual("{ class = pure-button prev, title = Go to page 1, data-link = /IndexView/RefreshTableBody?page=1&sortKey=id&sortDirection=desc }", pager.First.Html.ToString());
            Assert.AreEqual("fa fa-angle-double-left fa-lg", string.Join(" ", pager.First.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.Previous.Type);
            Assert.AreEqual("{ class = pure-button, title = Go to page 7, data-link = /IndexView/RefreshTableBody?page=7&sortKey=id&sortDirection=desc }", pager.Previous.Html.ToString());
            Assert.AreEqual("fa fa-angle-left fa-lg", string.Join(" ", pager.Previous.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.PagerState.Type);
            Assert.AreEqual("{ class = pure-button pure-button-disabled }", pager.PagerState.Html.ToString());
            Assert.AreEqual("8 of 10", pager.PagerState.Text);

            Assert.AreEqual(ActionType.Button, pager.Next.Type);
            Assert.AreEqual("{ class = pure-button, title = Go to page 9, data-link = /IndexView/RefreshTableBody?page=9&sortKey=id&sortDirection=desc }", pager.Next.Html.ToString());
            Assert.AreEqual("fa fa-angle-right fa-lg", string.Join(" ", pager.Next.IconHtmlClasses));

            Assert.AreEqual(ActionType.Button, pager.Last.Type);
            Assert.AreEqual("{ class = pure-button next, title = Go to page 10, data-link = /IndexView/RefreshTableBody?page=10&sortKey=id&sortDirection=desc }", pager.Last.Html.ToString());
            Assert.AreEqual("fa fa-angle-double-right fa-lg", string.Join(" ", pager.Last.IconHtmlClasses));
        }
    }

    [TestClass]
    public class AndBuildingAPagedIndexThatIsFiltered : AndBuildingAPagedIndexView
    {
        public override void Act()
        {
            base.Act();

            var keys = new List<string>();
            var values = new List<string>();

            for(int i = 1; i < 26; i++)
            {
                keys.Add("id");
                values.Add(i.ToString());
            }

            IndexView = Builder.BuildIndexView<TestEntity>(pageSize: 25, 
                filterKeys: keys, 
                filterValues: values);
        }

        [TestMethod]
        public void ThenTheIndexViewShouldHaveTheCorrectPagingButton()
        {
            AssertCommon();

            Assert.AreEqual(25, IndexView.Table.Rows.Count);

            var pager = IndexView.Pager;

            Assert.IsNull(pager.Previous);
            Assert.IsNull(pager.PagerState);
            Assert.IsNull(pager.Next);
        }
    }

    [TestClass]
    public class AndBuildingAPagedIndexThatIsFilteredAndHasMoreThanOnePage : AndBuildingAPagedIndexView
    {
        public override void Act()
        {
            base.Act();

            var keys = new List<string>();
            var values = new List<string>();

            for(int i = 1; i < 27; i++)
            {
                keys.Add("id");
                values.Add(i.ToString());
            }

            IndexView = Builder.BuildIndexView<TestEntity>(pageSize: 25,
                filterKeys: keys,
                filterValues: values);
        }

        [TestMethod]
        public void ThenTheIndexViewShouldHaveTheCorrectPagingButton()
        {
            AssertCommon();

            Assert.AreEqual(25, IndexView.Table.Rows.Count);

            var pager = IndexView.Pager;

            Assert.IsNull(pager.First);
            Assert.IsNull(pager.Previous);

            Assert.AreEqual(ActionType.Button, pager.PagerState.Type);
            Assert.AreEqual("{ class = pure-button pure-button-disabled }", pager.PagerState.Html.ToString());
            Assert.AreEqual("1 of 2", pager.PagerState.Text);

            Assert.AreEqual(ActionType.Button, pager.Next.Type);
            Assert.AreEqual("{ class = pure-button next, title = Go to page 2, data-link = /IndexView/RefreshTableBody?page=2&filterKeys%5B0%5D=id&filterValues%5B0%5D=1&filterKeys%5B1%5D=id&filterValues%5B1%5D=2&filterKeys%5B2%5D=id&filterValues%5B2%5D=3&filterKeys%5B3%5D=id&filterValues%5B3%5D=4&filterKeys%5B4%5D=id&filterValues%5B4%5D=5&filterKeys%5B5%5D=id&filterValues%5B5%5D=6&filterKeys%5B6%5D=id&filterValues%5B6%5D=7&filterKeys%5B7%5D=id&filterValues%5B7%5D=8&filterKeys%5B8%5D=id&filterValues%5B8%5D=9&filterKeys%5B9%5D=id&filterValues%5B9%5D=10&filterKeys%5B10%5D=id&filterValues%5B10%5D=11&filterKeys%5B11%5D=id&filterValues%5B11%5D=12&filterKeys%5B12%5D=id&filterValues%5B12%5D=13&filterKeys%5B13%5D=id&filterValues%5B13%5D=14&filterKeys%5B14%5D=id&filterValues%5B14%5D=15&filterKeys%5B15%5D=id&filterValues%5B15%5D=16&filterKeys%5B16%5D=id&filterValues%5B16%5D=17&filterKeys%5B17%5D=id&filterValues%5B17%5D=18&filterKeys%5B18%5D=id&filterValues%5B18%5D=19&filterKeys%5B19%5D=id&filterValues%5B19%5D=20&filterKeys%5B20%5D=id&filterValues%5B20%5D=21&filterKeys%5B21%5D=id&filterValues%5B21%5D=22&filterKeys%5B22%5D=id&filterValues%5B22%5D=23&filterKeys%5B23%5D=id&filterValues%5B23%5D=24&filterKeys%5B24%5D=id&filterValues%5B24%5D=25&filterKeys%5B25%5D=id&filterValues%5B25%5D=26 }", pager.Next.Html.ToString());
            Assert.AreEqual("fa fa-angle-right fa-lg", string.Join(" ", pager.Next.IconHtmlClasses));

            Assert.IsNull(pager.Last);
        }
    }

    [TestClass]
    public class AndBuildingTheIndexViewWithNullVisitors : AndBuildingTheIndexView
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.All("AnotherEntities", 
                "AnotherEntity", 
                "OtherAnotherEntities", 
                "RequiredEntity", 
                "ForeignKey"))
                .Returns(new List<TestEntity>
                {
                    new TestEntity
                    {
                        Id = 1,
                        Name = "Name of One",
                        AnotherEntity = new AnotherEntity
                        {
                            Id = 1,
                            StringProperty = "Another One"
                        },
                        ForeignKeyId = 1,
                        ForeignKey = new AnotherEntity
                        {
                            Id = 4,
                            StringProperty = "FK Four"
                        }
                    },
                    new TestEntity
                    {
                        Id = 2,
                        Name = "Name of Two",
                        AnotherEntities = new List<AnotherEntity>
                        {
                            new AnotherEntity
                            {
                                Id = 1,
                                StringProperty = "Another Entities One"
                            }
                        },
                        AnotherEntity = new AnotherEntity
                        {
                            Id = 2,
                            StringProperty = "Another Two"
                        }
                    }
                });
        }

        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(null);
        }

        [TestMethod]
        public void ThenTheCorrectIndexViewShouldBeReturned()
        {
            Assert.AreEqual(2, IndexView.Table.Rows.Count);

            var row = IndexView.Table.Rows[0];
            Assert.AreEqual(1, row.Id);
            Assert.AreEqual("{ data-link = /TestEntities/1 }", row.Html.ToString());
            Assert.AreEqual(8, row.Columns.Count);
            Assert.AreEqual("Name of One", row.Columns[0].Value);
            Assert.AreEqual(1, row.Columns[1].Value);
            Assert.AreEqual(string.Empty, row.Columns[2].Value);
            Assert.AreEqual(string.Empty, row.Columns[3].Value);
            Assert.AreEqual("Another One", row.Columns[4].Value);
            Assert.AreEqual(string.Empty, row.Columns[5].Value);
            Assert.AreEqual(string.Empty, row.Columns[6].Value);
            Assert.AreEqual("FK Four", row.Columns[7].Value);

            row = IndexView.Table.Rows[1];
            Assert.AreEqual(2, row.Id);
            Assert.AreEqual("{ class = pure-table-odd, data-link = /TestEntities/2 }", row.Html.ToString());
            Assert.AreEqual(8, row.Columns.Count);
            Assert.AreEqual("Name of Two", row.Columns[0].Value);
            Assert.AreEqual(2, row.Columns[1].Value);
            Assert.AreEqual(string.Empty, row.Columns[2].Value);
            Assert.AreEqual("Another Entities One", row.Columns[3].Value);
            Assert.AreEqual("Another Two", row.Columns[4].Value);
            Assert.AreEqual(string.Empty, row.Columns[5].Value);
            Assert.AreEqual(string.Empty, row.Columns[6].Value);
            Assert.AreEqual(string.Empty, row.Columns[7].Value);
        }
    }

    [TestClass]
    public class AndBuildingTheIndexViewWithNoNullValues : AndBuildingTheIndexView
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.All("AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey"))
                .Returns(new List<TestEntity>
                {
                    new TestEntity
                    {
                        Id = 1,
                        Name = "Name of One"
                    },
                    new TestEntity
                    {
                        Id = 2,
                        Name = "Name of Two"
                    }
                });
        }

        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(null, null, null, null, null, null, null, MockViewVisitor.Object);
        }

        [TestMethod]
        public void ThenTheCorrectIndexViewShouldBeReturned()
        {
            AssertCommon();

            Assert.AreEqual("{ data-link = /TestEntities/1 }", IndexView.Table.Rows[0].Html.ToString());
            Assert.AreEqual(8, IndexView.Table.Rows[0].Columns.Count);
            Assert.AreEqual("Name of One", IndexView.Table.Rows[0].Columns[0].Value);
            Assert.AreEqual(1, IndexView.Table.Rows[0].Columns[1].Value);

            Assert.AreEqual("{ class = pure-table-odd, data-link = /TestEntities/2 }", IndexView.Table.Rows[1].Html.ToString());
            Assert.AreEqual(8, IndexView.Table.Rows[1].Columns.Count);
            Assert.AreEqual("Name of Two", IndexView.Table.Rows[1].Columns[0].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[1].Columns[1].Value);

            MockViewVisitor.Verify(v => v.Visit(It.IsAny<Column>()), Times.Exactly(16));
            MockViewVisitor.Verify(v => v.Visit(It.IsAny<Row>()), Times.Exactly(2));
        }
    }

    [TestClass]
    public class AndBuildingTheIndexViewWithNoNullValuesAndHasCollectionProperties : AndBuildingTheIndexView
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.All("AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey"))
                .Returns(new List<TestEntity>
                {
                    new TestEntity
                    {
                        Id = 1,
                        Name = "Name of One",
                        AnotherEntities = new List<AnotherEntity>
                        {
                            new AnotherEntity
                            {
                                Id = 1,
                                StringProperty = "Another Entity One"
                            },
                            new AnotherEntity
                            {
                                Id = 2,
                                StringProperty = "Another Entity Two"
                            }
                        }
                    },
                    new TestEntity
                    {
                        Id = 2,
                        Name = "Name of Two"
                    }
                });
        }

        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(null, null, null, null, null, null, null, MockViewVisitor.Object);
        }

        [TestMethod]
        public void ThenTheCorrectIndexViewShouldBeReturned()
        {
            AssertCommon();

            Assert.AreEqual("{ data-link = /TestEntities/1 }", IndexView.Table.Rows[0].Html.ToString());
            Assert.AreEqual(8, IndexView.Table.Rows[0].Columns.Count);
            Assert.AreEqual("Name of One", IndexView.Table.Rows[0].Columns[0].Value);
            Assert.AreEqual(1, IndexView.Table.Rows[0].Columns[1].Value);
            Assert.AreEqual("Another Entity One, Another Entity Two", IndexView.Table.Rows[0].Columns[3].Value);

            Assert.AreEqual("{ class = pure-table-odd, data-link = /TestEntities/2 }", IndexView.Table.Rows[1].Html.ToString());
            Assert.AreEqual(8, IndexView.Table.Rows[1].Columns.Count);
            Assert.AreEqual("Name of Two", IndexView.Table.Rows[1].Columns[0].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[1].Columns[1].Value);

            MockViewVisitor.Verify(v => v.Visit(It.IsAny<Column>()), Times.Exactly(16));
            MockViewVisitor.Verify(v => v.Visit(It.IsAny<Row>()), Times.Exactly(2));
        }
    }

    [TestClass]
    public class AndBuildingTheIndexViewWithNullValues : AndBuildingTheIndexView
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.All("AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey"))
                .Returns(new List<TestEntity>
                {
                    new TestEntity
                    {
                        Id = 1
                    },
                    new TestEntity
                    {
                        Id = 2,
                        Name = "Name of Two"
                    }
                });
        }

        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(null, null, null, null, null, null, null, MockViewVisitor.Object);
        }

        [TestMethod]
        public void ThenTheCorrectIndexViewShouldBeReturned()
        {
            AssertCommon();

            Assert.AreEqual(8, IndexView.Table.Rows[0].Columns.Count);
            Assert.AreEqual(string.Empty, IndexView.Table.Rows[0].Columns[0].Value);
            Assert.AreEqual("{ data-link = /TestEntities/1 }", IndexView.Table.Rows[0].Html.ToString());

            Assert.AreEqual(8, IndexView.Table.Rows[1].Columns.Count);
            Assert.AreEqual("Name of Two", IndexView.Table.Rows[1].Columns[0].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[1].Columns[1].Value);
            Assert.AreEqual("{ class = pure-table-odd, data-link = /TestEntities/2 }", IndexView.Table.Rows[1].Html.ToString());

            MockViewVisitor.Verify(v => v.Visit(It.IsAny<Column>()), Times.Exactly(16));
            MockViewVisitor.Verify(v => v.Visit(It.IsAny<Row>()), Times.Exactly(2));
        }
    }

    [TestClass]
    public class AndBuildingTheIndexViewWithEmptyRows : BehaviorDrivenDevelopmentCaseTemplate
    {
        CrudViewBuilder _builder;
        IndexView _indexView;
        Mock<IIndexViewVisitor> _mockViewVisitor;
        Mock<IRepositoryFactory> _mockRepositoryFactory;

        public override void Arrange()
        {
            base.Arrange();

            _mockRepositoryFactory = new Mock<IRepositoryFactory>();

            _mockRepositoryFactory.Setup(r => r.Get<NullableTestEntity>().All())
                .Returns(new List<NullableTestEntity>
                {
                    new NullableTestEntity(),
                    new NullableTestEntity
                    {
                        Id = 2,
                        Name = "Name of Two"
                    }
                });

            _builder = new CrudViewBuilder(_mockRepositoryFactory.Object);
            _mockViewVisitor = new Mock<IIndexViewVisitor>();

            RouteTable.Routes.Clear();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var mockHttpRequest = new Mock<HttpRequestBase>();
            mockHttpRequest.Setup(r => r.Url).Returns(new Uri("https://localhost.com:443"));

            var mockHttpResponse = new Mock<HttpResponseBase>();
            mockHttpResponse.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(s => s);

            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext
                .Setup(ctx => ctx.Request)
                .Returns(mockHttpRequest.Object);
            mockHttpContext
                .Setup(ctx => ctx.Response)
                .Returns(mockHttpResponse.Object);

            RequestContextAdapter.Context = new RequestContext(mockHttpContext.Object, new RouteData());
        }

        public override void Act()
        {
            base.Act();

            _indexView = _builder.BuildIndexView<NullableTestEntity>(null, null, null, null, null, null, null, _mockViewVisitor.Object);
        }

        [TestMethod]
        public void ThenTheCorrectIndexViewShouldBeReturned()
        {
            Assert.AreEqual("NullableTestEntities", _indexView.Title);
            Assert.AreEqual(2, _indexView.Table.Rows.Count);

            Assert.IsNull(_indexView.Table.Rows[0].Html["data-link"]);

            Assert.AreEqual("Name", _indexView.Table.Header.Columns[0].Value);
            Assert.AreEqual("Id", _indexView.Table.Header.Columns[1].Value);

            Assert.AreEqual(2, _indexView.Table.Rows[0].Columns.Count);
            Assert.AreEqual(string.Empty, _indexView.Table.Rows[0].Columns[0].Value);

            Assert.AreEqual("{ class = pure-table-odd, data-link = /NullableTestEntities/2 }", _indexView.Table.Rows[1].Html.ToString());
            Assert.AreEqual(2, _indexView.Table.Rows[1].Columns.Count);
            Assert.AreEqual("Name of Two", _indexView.Table.Rows[1].Columns[0].Value);
            Assert.AreEqual(2, _indexView.Table.Rows[1].Columns[1].Value);

            _mockViewVisitor.Verify(v => v.Visit(_indexView.Table), Times.Once());
            _mockViewVisitor.Verify(v => v.Visit(It.IsAny<Column>()), Times.Exactly(4));
            _mockViewVisitor.Verify(v => v.Visit(It.IsAny<Row>()), Times.Exactly(2));
        }
    }

    #region Sorting

    public class AndSorting : AndBuildingTheIndexView
    {
        protected string SortKey;
        protected string SortDirection;

        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.All("AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey"))
                .Returns(new List<TestEntity>
                {
                    new TestEntity
                    {
                        Id = 2,
                        Name = "Name of One",
                        NamedName = "One",
                        AnotherEntity = new AnotherEntity
                        {
                            Id = 1,
                            StringProperty = "Another Entity"
                        }
                    },
                    new TestEntity
                    {
                        Id = 3,
                        Name = "Two",
                        NamedName = "Name Two"
                    },
                    new TestEntity
                    {
                        Id = 1,
                        Name = "Another one",
                        NamedName = "Another one"
                    }
                });
        }

        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(null, null, SortKey, SortDirection);
        }

        protected override void AssertCommon()
        {
            Assert.AreEqual<string>("TestEntities", IndexView.Title);
            Assert.AreEqual(3, IndexView.Table.Rows.Count);
            Assert.AreEqual(typeof(TestEntity).AssemblyQualifiedName, IndexView.ModelName);
        }
    }

    [TestClass]
    public class WhenBothKeyAndDescendingDirectionAreGiven : AndSorting
    {
        public override void Arrange()
        {
            base.Arrange();

            SortKey = "name";
            SortDirection = "desc";
        }

        [TestMethod]
        public void ThenTheViewShouldBeSortedInDescendingOrder()
        {
            AssertCommon();

            Assert.AreEqual(3, IndexView.Table.Rows[0].Columns[1].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[1].Columns[1].Value);
            Assert.AreEqual(1, IndexView.Table.Rows[2].Columns[1].Value);
        }
    }

    [TestClass]
    public class WhenBothKeyAndAscendingDirectionAreGiven : AndSorting
    {
        public override void Arrange()
        {
            base.Arrange();

            SortKey = "name";
            SortDirection = "asc";
        }

        [TestMethod]
        public void ThenTheViewShouldBeSortedInAscendingOrder()
        {
            AssertCommon();

            Assert.AreEqual(1, IndexView.Table.Rows[0].Columns[1].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[1].Columns[1].Value);
            Assert.AreEqual(3, IndexView.Table.Rows[2].Columns[1].Value);
        }
    }

    [TestClass]
    public class WhenBothKeyAndNoDirectionAreGiven : AndSorting
    {
        public override void Arrange()
        {
            base.Arrange();

            SortKey = "name";
            SortDirection = string.Empty;
        }

        [TestMethod]
        public void ThenTheViewShouldBeSortedInAscendingOrder()
        {
            AssertCommon();

            Assert.AreEqual(1, IndexView.Table.Rows[0].Columns[1].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[1].Columns[1].Value);
            Assert.AreEqual(3, IndexView.Table.Rows[2].Columns[1].Value);
        }
    }

    [TestClass]
    public class WhenAnInvalidSortKeyIsGiven : AndSorting
    {
        public override void Arrange()
        {
            base.Arrange();

            SortKey = "nonexistent";
        }

        public override void Act()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenAnInvalidOperationExceptionShouldBeThrown()
        {
            Builder.BuildIndexView<TestEntity>(sortKey: SortKey);
            Assert.Fail();
        }
    }

    [TestClass]
    public class WhenAnInvalidSortDirectionIsGiven : AndSorting
    {
        public override void Arrange()
        {
            base.Arrange();

            SortKey = "name";
            SortDirection = "invalid";
        }

        public override void Act()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenAnInvalidOperationExceptionShouldBeThrown()
        {
            Builder.BuildIndexView<TestEntity>(null, null, SortKey, SortDirection);
            Assert.Fail();
        }
    }

    [TestClass]
    public class WhenKeyIsThePropertyDisplayName : AndSorting
    {
        public override void Arrange()
        {
            base.Arrange();

            SortKey = "naMed namE";
        }

        [TestMethod]
        public void ThenTheViewShouldBeSortedInAscendingOrder()
        {
            AssertCommon();

            Assert.AreEqual(1, IndexView.Table.Rows[0].Columns[1].Value);
            Assert.AreEqual(3, IndexView.Table.Rows[1].Columns[1].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[2].Columns[1].Value);
        }
    }

    [TestClass]
    public class WhenKeyIsComplexPropertyAndNotUsingDisplayName : AndSorting
    {
        public override void Arrange()
        {
            base.Arrange();

            SortKey = "AnotherEntity";
        }

        [TestMethod]
        public void ThenTheViewShouldBeSortedInAscendingOrder()
        {
            AssertCommon();

            Assert.AreEqual(3, IndexView.Table.Rows[0].Columns[1].Value);
            Assert.AreEqual(1, IndexView.Table.Rows[1].Columns[1].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[2].Columns[1].Value);
        }
    }

    [TestClass]
    public class WhenASearchQueryIsIncluded : AndSorting
    {
        public override void Arrange()
        {
            base.Arrange();

            SortKey = "name";
            SortDirection = "desc";
        }

        public override void Act()
        {
            IndexView = Builder.BuildIndexView<TestEntity>(null, null, SortKey, SortDirection, searchQuery: "1");
        }

        [TestMethod]
        public void ThenTheViewShouldBeSortedInDescendingOrder()
        {
            AssertCommon();

            Assert.AreEqual(3, IndexView.Table.Rows[0].Columns[1].Value);
            Assert.AreEqual(2, IndexView.Table.Rows[1].Columns[1].Value);
            Assert.AreEqual(1, IndexView.Table.Rows[2].Columns[1].Value);
        }
    }

    #endregion

    #region Filtering

    public class AndFiltering : AndBuildingTheIndexView
    {
        protected List<string> FilterKeys;
        protected List<string> FilterValues;

        public override void Arrange()
        {
            base.Arrange();

            MockRepository.Setup(r => r.All("AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey"))
                .Returns(new List<TestEntity>
                {
                    new TestEntity
                    {
                        Id = 2,
                        Name = "Name of One",
                        NamedName = "One",
                    },
                    new TestEntity
                    {
                        Id = 3,
                        Name = "Two",
                        NamedName = "Name Two"
                    },
                    new TestEntity
                    {
                        Id = 1,
                        Name = "Another name",
                        NamedName = "Another one"
                    },
                    new TestEntity
                    {
                        Id = 4,
                        Name = "Another name",
                        NamedName = "Another one"
                    }
                });
        }

        public override void Act()
        {
            base.Act();

            IndexView = Builder.BuildIndexView<TestEntity>(null, null, null, null, FilterKeys, FilterValues);
        }

        protected override void AssertCommon()
        {
            Assert.AreEqual<string>("TestEntities", IndexView.Title);
            Assert.AreEqual(typeof(TestEntity).AssemblyQualifiedName, IndexView.ModelName);
        }
    }

    [TestClass]
    public class AndBothKeyAndValuesAreGiven : AndFiltering
    {
        public override void Arrange()
        {
            base.Arrange();

            FilterKeys = new List<string>
            {
                "id"
            };

            FilterValues = new List<string>
            {
                "1"
            };
        }

        [TestMethod]
        public void ThenTheViewShouldBeFilteredAccordingly()
        {
            Assert.AreEqual(1, IndexView.Table.Rows.Count);
            Assert.AreEqual(1, IndexView.Table.Rows[0].Columns[1].Value);
        }
    }

    [TestClass]
    public class AndValueDoesntExist : AndFiltering
    {
        public override void Arrange()
        {
            base.Arrange();

            FilterKeys = new List<string>
            {
                "id"
            };

            FilterValues = new List<string>
            {
                "6"
            };
        }

        [TestMethod]
        public void ThenTheViewShouldBeFilteredAccordingly()
        {
            Assert.AreEqual(0, IndexView.Table.Rows.Count);
        }
    }

    [TestClass]
    public class AndADisplayNameIsUsedAsAKey : AndFiltering
    {
        public override void Arrange()
        {
            base.Arrange();

            FilterKeys = new List<string>
            {
                "name",
                "named name"
            };

            FilterValues = new List<string>
            {
                "Another name",
                "Another one"
            };
        }

        [TestMethod]
        public void ThenTheViewShouldBeFilteredAccordingly()
        {
            Assert.AreEqual(2, IndexView.Table.Rows.Count);

            Assert.AreEqual("Another one", IndexView.Table.Rows[0].Columns[2].Value);
            Assert.AreEqual("Another one", IndexView.Table.Rows[1].Columns[2].Value);
        }
    }

    [TestClass]
    public class WhenKeysAndValuesDontHaveSameCountOfItems : AndFiltering
    {
        public override void Arrange()
        {
            base.Arrange();

            FilterKeys = new List<string>
            {
                "invalid",
                "named name"
            };

            FilterValues = new List<string>();
        }

        public override void Act()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenAnInvalidOperationExceptionShouldBeThrown()
        {
            Builder.BuildIndexView<TestEntity>(null, null, null, null, FilterKeys, FilterValues);
            Assert.Fail();
        }
    }

    [TestClass]
    public class WhenKeyIsInvalid : AndFiltering
    {
        public override void Arrange()
        {
            base.Arrange();

            FilterKeys = new List<string>
            {
                "invalid",
                "named name"
            };

            FilterValues = new List<string>
            {
                "1",
                "2",
            };
        }

        public override void Act()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenAnInvalidOperationExceptionShouldBeThrown()
        {
            Builder.BuildIndexView<TestEntity>(null, null, null, null, FilterKeys, FilterValues);
            Assert.Fail();
        }
    }


    #endregion

    #endregion

    #region GetIndexFilterOptions

    public class AndGettingIndexFilterOptions : WhenWorkingWithTheCrudViewBuilder
    {
        protected List<FilterOption> Results;
        protected List<TestEntity> EntityList;

        public override void Arrange()
        {
            base.Arrange();

            EntityList = new List<TestEntity>
            {
                new TestEntity
                {
                    Id = 1,
                    Name = "Name",
                    NamedName = "NamedName",
                    NotMapped = "NotMapped",
                    AnotherEntities = new List<AnotherEntity>
                    {
                        new AnotherEntity { Id = 1, StringProperty = "Another Entity One" },
                        new AnotherEntity { Id = 2, StringProperty = "Another Entity Two" }
                    }
                },
                new TestEntity
                {
                    Id = 2
                }
            };

            MockRepository.Setup(r => r.All("AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "RequiredEntity",
                "ForeignKey")).Returns(EntityList);
        }

        protected void AssertCommon()
        {
            Assert.AreEqual(8, Results.Count());

            Assert.AreEqual("Name", Results.ElementAt(0).Label);
            Assert.AreEqual("Name", Results.ElementAt(0).Children.ElementAt(0).Label);

            Assert.AreEqual("Id", Results.ElementAt(1).Label);
            Assert.AreEqual("1", Results.ElementAt(1).Children.ElementAt(0).Label);

            Assert.AreEqual("Named Name", Results.ElementAt(2).Label);
            Assert.AreEqual("NamedName", Results.ElementAt(2).Children.ElementAt(0).Label);

            Assert.AreEqual("Another Entity", Results.ElementAt(3).Label);
            Assert.AreEqual("Another Entity One, Another Entity Two", Results.ElementAt(3).Children.ElementAt(0).Label);

            Assert.AreEqual("AnotherEntity", Results.ElementAt(4).Label);

            Assert.AreEqual("Other Another Entity", Results.ElementAt(5).Label);
        }
    }

    [TestClass]
    public class WithNoVisitorsBeingPassed : AndGettingIndexFilterOptions
    {
        public override void Act()
        {
            base.Act();

            Results = Builder.GetIndexFilterOptions<TestEntity>().ToList();
        }

        [TestMethod]
        public void ThenAMapOfFilterOptionsShouldBeReturned()
        {
            AssertCommon();
        }
    }

    [TestClass]
    public class WithAVisitorBeingPassed : AndGettingIndexFilterOptions
    {
        Mock<IFilterOptionVisitor> _mockVisitor;

        public override void Arrange()
        {
            base.Arrange();

            _mockVisitor = new Mock<IFilterOptionVisitor>();
            _mockVisitor.Setup(v => v.Visit(It.IsAny<TestEntity>())).Returns((IIdentifiable item) => item);
        }

        public override void Act()
        {
            base.Act();

            Results = Builder.GetIndexFilterOptions<TestEntity>(_mockVisitor.Object).ToList();
        }

        [TestMethod]
        public void ThenTheVisitorsVisitMethodShouldBeCalled()
        {
            AssertCommon();

            Assert.AreEqual(2, Results.ElementAt(1).Children.Count());

            for(int i = 0; i < EntityList.Count; i++)
            {
                var result = EntityList[i] as IIdentifiable;
                _mockVisitor.Verify(v => v.Visit(result), Times.Once());
            }
        }
    }

    [TestClass]
    public class WithAVisitorBeingPassedThatReturnsANullOption : AndGettingIndexFilterOptions
    {
        Mock<IFilterOptionVisitor> _mockVisitor;

        public override void Arrange()
        {
            base.Arrange();

            _mockVisitor = new Mock<IFilterOptionVisitor>();
            _mockVisitor.Setup(v => v.Visit(It.Is<TestEntity>(e => e.Id == 2))).Returns(() => null);
            _mockVisitor.Setup(v => v.Visit(It.Is<TestEntity>(e => e.Id != 2))).Returns((IIdentifiable item) => item);
        }

        public override void Act()
        {
            base.Act();

            Results = Builder.GetIndexFilterOptions<TestEntity>(_mockVisitor.Object).ToList();
        }

        [TestMethod]
        public void ThenTheVisitorsVisitMethodShouldBeCalledAndTheNullOptionShouldNotBeAdded()
        {
            AssertCommon();

            Assert.AreEqual(1, Results.ElementAt(1).Children.Count());

            for(int i = 0; i < EntityList.Count; i++)
            {
                var result = EntityList[i] as IIdentifiable;
                _mockVisitor.Verify(v => v.Visit(result), Times.Once());
            }
        }
    }

    #endregion

    #region DetailsView

    public class AndBuildingTheDetailsView : WhenWorkingWithTheCrudViewBuilder
    {
        protected DetailsView DetailsView;
        protected TestEntity Entity;
        protected Mock<IDetailsViewVisitor> MockViewVisitor;

        public override void Arrange()
        {
            base.Arrange();

            Entity = new TestEntity
            {
                Id = 1,
                Name = "Entity One",
                NamedName = "Named One",
                AnotherEntities = new List<AnotherEntity>
                {
                    new AnotherEntity 
                    { 
                        Id = 1, 
                        StringProperty = "Another Property One" 
                    },
                    new AnotherEntity 
                    { 
                        Id = 2, 
                        StringProperty = "Another Property Two" 
                    }
                }
            };

            MockRepositoryFactory.Setup(f => f.Get<TestEntity>()
                .Find(It.IsAny<Expression<Func<TestEntity, bool>>>(), "AnotherEntities", 
                "AnotherEntity", 
                "OtherAnotherEntities", 
                "ForeignKey"))
                .Returns(new List<TestEntity>
            {
                Entity
            });

            MockViewVisitor = new Mock<IDetailsViewVisitor>();
        }

        public override void Act()
        {
            base.Act();

            DetailsView = Builder.BuildDetailsView<TestEntity>(1, MockViewVisitor.Object);
        }

        protected void AssertCommon()
        {
            Assert.AreEqual("Entity One", DetailsView.Title);
            Assert.AreEqual(7, DetailsView.Table.Rows.Count);

            var action = DetailsView.ContextItems[0] as ActionModel;
            Assert.AreEqual(ActionType.Link, action.Type);
            Assert.AreEqual("Default", action.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Index }", action.RouteValues.ToString());
            Assert.AreEqual("{ class = pure-button pure-button-primary, title = Back to TestEntities }", action.Html.ToString());
            Assert.AreEqual("fa fa-level-up fa-lg", action.IconHtmlClasses[0]);

            action = DetailsView.ContextItems[1] as ActionModel;
            Assert.AreEqual(ActionType.Link, action.Type);
            Assert.AreEqual("Details", action.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Update, id = 1 }", action.RouteValues.ToString());
            Assert.AreEqual("{ class = pure-button, title = Update Entity One }", action.Html.ToString());
            Assert.AreEqual("fa fa-edit fa-lg", action.IconHtmlClasses[0]);

            var form = DetailsView.ContextItems[2] as Form;
            Assert.AreEqual(FormMethod.Post, form.FormMethod);
            Assert.AreEqual("Details", form.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Delete, id = 1 }", form.RouteValues.ToString());
            Assert.AreEqual("{ class = delete, style = display: inline-block; }", form.Html.ToString());

            Assert.AreEqual(ActionType.Button, form.Submit.Type);
            Assert.AreEqual("{ class = pure-button button-warning, title = Delete Entity One, type = submit }", form.Submit.Html.ToString());
            Assert.AreEqual("fa fa-trash-o fa-lg", form.Submit.IconHtmlClasses[0]);

            MockViewVisitor.Verify(v => v.Visit(DetailsView.Table), Times.Once());
            MockViewVisitor.Verify(v => v.Visit(DetailsView), Times.Once());

            foreach(var row in DetailsView.Table.Rows)
            {
                MockViewVisitor.Verify(v => v.Visit(row), Times.Once());
                Assert.IsNotNull(row.Source);

                foreach(var column in row.Columns)
                {
                    MockViewVisitor.Verify(v => v.Visit(column), Times.Once());
                    Assert.IsNotNull(column.Source);
                }
            }
        }
    }

    [TestClass]
    public class WhenTheEntityExists : AndBuildingTheDetailsView
    {
        [TestMethod]
        public void ThenTheTableShouldBeProperlyConstructed()
        {
            AssertCommon();

            Assert.AreEqual("Name", DetailsView.Table.Rows[0].Columns[0].Value);
            Assert.AreEqual("Entity One", DetailsView.Table.Rows[0].Columns[1].Value);

            Assert.AreEqual("Id", DetailsView.Table.Rows[1].Columns[0].Value);
            Assert.AreEqual(1, DetailsView.Table.Rows[1].Columns[1].Value);

            Assert.AreEqual("Named Name", DetailsView.Table.Rows[2].Columns[0].Value);
            Assert.AreEqual("{ style = font-weight:bold; }", DetailsView.Table.Rows[2].Columns[0].Html.ToString());
            Assert.AreEqual("Named One", DetailsView.Table.Rows[2].Columns[1].Value);

            Assert.AreEqual("Another Entity", DetailsView.Table.Rows[3].Columns[0].Value);
            Assert.AreEqual("{ style = font-weight:bold; }", DetailsView.Table.Rows[3].Columns[0].Html.ToString());
            Assert.AreEqual("Another Property One, Another Property Two", DetailsView.Table.Rows[3].Columns[1].Value);
        }
    }

    [TestClass]
    public class WhenTheEntityDoesNotExist : AndBuildingTheDetailsView
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepositoryFactory.Setup(f => f.Get<TestEntity>()
                .Find(It.IsAny<Expression<Func<TestEntity, bool>>>(), "AnotherEntities",
                "AnotherEntity",
                "OtherAnotherEntities",
                "ForeignKey"))
                .Returns(new List<TestEntity>());
        }

        public override void Act()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenAnInvalidOperationExceptionShouldBeThrown()
        {
            var view = Builder.BuildDetailsView<TestEntity>(1);
            Assert.Fail();
        }
    }

    #endregion

    #region UpdateView

    public class AndBuildingTheUpdateView : WhenWorkingWithTheCrudViewBuilder
    {
        protected UpdateView UpdateView;
        protected TestEntity Entity;

        public override void Arrange()
        {
            base.Arrange();

            Entity = new TestEntity
            {
                Id = 1,
                Name = "Entity One",
                NamedName = "Named One",
                IsInUse = true,
                AnotherEntities = new List<AnotherEntity>
                {
                    new AnotherEntity { Id = 1, StringProperty = "Another One" },
                    new AnotherEntity { Id = 3, StringProperty = "Another Three" }
                },
                AnotherEntity = new AnotherEntity
                {
                    Id = 1
                }
            };

            MockRepositoryFactory.Setup(f => f.Get<TestEntity>()
                .Find(It.IsAny<Expression<Func<TestEntity, bool>>>(), 
                "AnotherEntities", 
                "AnotherEntity", 
                "OtherAnotherEntities", 
                "RequiredEntity", 
                "ForeignKey"))
                .Returns(new List<TestEntity>
            {
                Entity
            });

            MockRepositoryFactory.Setup(f => f.Get<AnotherEntity>().All(It.IsAny<string[]>())).Returns(new List<AnotherEntity>
            {
                new AnotherEntity { Id = 1, StringProperty = "Another One" },
                new AnotherEntity { Id = 2, StringProperty = "Another Two" },
                new AnotherEntity { Id = 3, StringProperty = "Another Three" }
            });
        }

        public override void Act()
        {
            base.Act();

            UpdateView = Builder.BuildUpdateView<TestEntity>(1);
        }

        protected void AssertCommon()
        {
            Assert.AreEqual("Entity One", UpdateView.Title);
            Assert.AreEqual(8, UpdateView.InputProperties.Count);

            var action = UpdateView.ContextItems[0] as ActionModel;
            Assert.AreEqual(ActionType.Button, action.Type);
            Assert.AreEqual("{ id = saveButton, class = pure-button pure-button-primary, title = Save }", action.Html.ToString());
            Assert.AreEqual("fa fa-floppy-o fa-lg", string.Join(" ", action.IconHtmlClasses));

            action = UpdateView.ContextItems[1] as ActionModel;
            Assert.AreEqual(ActionType.Link, action.Type);
            Assert.AreEqual("{ class = pure-button, title = Cancel }", action.Html.ToString());
            Assert.AreEqual("fa fa-times fa-lg", string.Join(" ", action.IconHtmlClasses));
            Assert.AreEqual("Default", action.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Index }", action.RouteValues.ToString());

            Assert.AreEqual("{ class = pure-form pure-form-aligned }", UpdateView.Form.Html.ToString());
            Assert.AreEqual("Details", UpdateView.Form.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Update, id = 1 }", UpdateView.Form.RouteValues.ToString());
            Assert.AreEqual(FormMethod.Post, UpdateView.Form.FormMethod);

            Assert.AreEqual("Name *", UpdateView.InputProperties[0].Label);
            Assert.AreEqual("Name", UpdateView.InputProperties[0].ModelName);
            Assert.AreEqual(InputPropertyType.String, UpdateView.InputProperties[0].Type);
            Assert.AreEqual("Entity One", UpdateView.InputProperties[0].Value);

            Assert.AreEqual("Named Name", UpdateView.InputProperties[1].Label);
            Assert.AreEqual("NamedName", UpdateView.InputProperties[1].ModelName);
            Assert.AreEqual(InputPropertyType.String, UpdateView.InputProperties[1].Type);
            Assert.AreEqual("Named One", UpdateView.InputProperties[1].Value);

            Assert.AreEqual("Is In Use", UpdateView.InputProperties[2].Label);
            Assert.AreEqual("IsInUse", UpdateView.InputProperties[2].ModelName);
            Assert.AreEqual(InputPropertyType.Basic, UpdateView.InputProperties[2].Type);
            Assert.AreEqual(true, UpdateView.InputProperties[2].Value);

            Assert.AreEqual("Another Entity *", UpdateView.InputProperties[3].Label);
            Assert.AreEqual("AnotherEntities", UpdateView.InputProperties[3].ModelName);
            Assert.AreEqual(InputPropertyType.Basic, UpdateView.InputProperties[3].Type);

            var select = UpdateView.InputProperties[3].Value as MultiSelectList;
            Assert.AreEqual("StringProperty", select.DataTextField);
            Assert.AreEqual("Id", select.DataValueField);
            Assert.AreEqual(3, select.Items.OfType<AnotherEntity>().Count());

            select = UpdateView.InputProperties[4].Value as SelectList;
            Assert.AreEqual("StringProperty", select.DataTextField);
            Assert.AreEqual("Id", select.DataValueField);
            Assert.AreEqual(4, select.Items.Count());

            Assert.AreEqual("RequiredEntity *", UpdateView.InputProperties[6].Label);
            select = UpdateView.InputProperties[6].Value as SelectList;
            Assert.AreEqual("StringProperty", select.DataTextField);
            Assert.AreEqual("Id", select.DataValueField);
            Assert.AreEqual(3, select.Items.Count());

            select = UpdateView.InputProperties[7].Value as SelectList;
            Assert.AreEqual("StringProperty", select.DataTextField);
            Assert.AreEqual("Id", select.DataValueField);
            Assert.AreEqual(4, select.Items.Count());

            foreach(var property in UpdateView.InputProperties)
            {
                Assert.AreEqual(1, (property.Source as TestEntity).Id);
                Assert.AreEqual("Entity One", (property.Source as TestEntity).Name);
            }
        }
    }

    [TestClass]
    public class WhenGivenANullId : AndBuildingTheUpdateView
    {
        public override void Act()
        {
            UpdateView = Builder.BuildUpdateView<TestEntity>(id: null);
        }

        [TestMethod]
        public void ThenTheRepositoryShouldNotBeSearchedAndUpdateViewShouldIndicateNewEntity()
        {
            Assert.AreEqual("Create new TestEntity", UpdateView.Title);
            Assert.AreEqual("Default", UpdateView.Form.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Update }", UpdateView.Form.RouteValues.ToString());

            MockRepository.Verify(r => r.Find(It.IsAny<object[]>()), Times.Never());
        }
    }

    [TestClass]
    public class WhenGivenANullModel : AndBuildingTheUpdateView
    {
        public override void Act()
        {

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenAnInvalidOperationExecptionShouldBeThrown()
        {
            Builder.BuildUpdateView<TestEntity>(model: null);
            Assert.Fail();
        }
    }

    [TestClass]
    public class WhenRepositoryCantFindTheEntity : AndBuildingTheUpdateView
    {
        public override void Arrange()
        {
            base.Arrange();

            MockRepositoryFactory.Setup(f => f.Get<TestEntity>()
                .Find(It.IsAny<Expression<Func<TestEntity, bool>>>(), 
                    "AnotherEntities", 
                    "AnotherEntity", 
                    "OtherAnotherEntities", 
                    "RequiredEntity", 
                    "ForeignKey"))
                .Returns(new List<TestEntity>());
        }

        public override void Act()
        {

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenAnInvalidOperationExecptionShouldBeThrown()
        {
            Builder.BuildUpdateView<TestEntity>(1);
            Assert.Fail();
        }
    }

    [TestClass]
    public class OfATestEntityWithSelectedValues : AndBuildingTheUpdateView
    {
        [TestMethod]
        public void TheTheCorrectUpdateViewShouldBeReturned()
        {
            AssertCommon();

            var selectedItems = (UpdateView.InputProperties[3].Value as MultiSelectList).SelectedValues.OfType<int>();

            Assert.AreEqual(2, selectedItems.Count());
            Assert.AreEqual(1, selectedItems.ElementAt(0));
            Assert.AreEqual(3, selectedItems.ElementAt(1));

            var selected = (UpdateView.InputProperties[4].Value as SelectList).SelectedValue;

            Assert.AreEqual(1, selected);
        }
    }

    [TestClass]
    public class OfATestEntityWithoutSelectedValues : AndBuildingTheUpdateView
    {
        public override void Arrange()
        {
            base.Arrange();

            Entity.AnotherEntities.Clear();
            Entity.AnotherEntity = null;
        }

        [TestMethod]
        public void TheTheCorrectUpdateViewShouldBeReturned()
        {
            AssertCommon();
            Assert.AreEqual(0, (UpdateView.InputProperties[3].Value as MultiSelectList).SelectedValues.OfType<AnotherEntity>().Count());
            Assert.AreEqual(null, (UpdateView.InputProperties[4].Value as SelectList).SelectedValue);
        }
    }

    [TestClass]
    public class OfATestEntityWithEmptyUpdatableFields : AndBuildingTheUpdateView
    {
        public override void Arrange()
        {
            base.Arrange();

            Entity.Name = null;
            Entity.NamedName = null;
        }

        [TestMethod]
        public void TheTheCorrectUpdateViewShouldBeReturned()
        {
            Assert.AreEqual("TestEntity", UpdateView.Title);
            Assert.AreEqual(8, UpdateView.InputProperties.Count);

            var action = UpdateView.ContextItems[0] as ActionModel;
            Assert.AreEqual(ActionType.Button, action.Type);
            Assert.AreEqual("{ id = saveButton, class = pure-button pure-button-primary, title = Save }", action.Html.ToString());
            Assert.AreEqual("fa fa-floppy-o fa-lg", string.Join(" ", action.IconHtmlClasses));

            action = UpdateView.ContextItems[1] as ActionModel;
            Assert.AreEqual(ActionType.Link, action.Type);
            Assert.AreEqual("{ class = pure-button, title = Cancel }", action.Html.ToString());
            Assert.AreEqual("fa fa-times fa-lg", string.Join(" ", action.IconHtmlClasses));
            Assert.AreEqual("Default", action.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Index }", action.RouteValues.ToString());

            Assert.AreEqual("{ class = pure-form pure-form-aligned }", UpdateView.Form.Html.ToString());
            Assert.AreEqual("Details", UpdateView.Form.RouteName);
            Assert.AreEqual("{ controller = TestEntities, action = Update, id = 1 }", UpdateView.Form.RouteValues.ToString());
            Assert.AreEqual(FormMethod.Post, UpdateView.Form.FormMethod);

            Assert.AreEqual("Name *", UpdateView.InputProperties[0].Label);
            Assert.AreEqual("Name", UpdateView.InputProperties[0].ModelName);
            Assert.AreEqual(InputPropertyType.String, UpdateView.InputProperties[0].Type);
            Assert.AreEqual(null, UpdateView.InputProperties[0].Value);

            Assert.AreEqual("Named Name", UpdateView.InputProperties[1].Label);
            Assert.AreEqual("NamedName", UpdateView.InputProperties[1].ModelName);
            Assert.AreEqual(InputPropertyType.String, UpdateView.InputProperties[1].Type);
            Assert.AreEqual(null, UpdateView.InputProperties[1].Value);

            Assert.AreEqual("Is In Use", UpdateView.InputProperties[2].Label);
            Assert.AreEqual("IsInUse", UpdateView.InputProperties[2].ModelName);
            Assert.AreEqual(InputPropertyType.Basic, UpdateView.InputProperties[2].Type);
            Assert.AreEqual(true, UpdateView.InputProperties[2].Value);

            Assert.AreEqual("Another Entity *", UpdateView.InputProperties[3].Label);
            Assert.AreEqual("AnotherEntities", UpdateView.InputProperties[3].ModelName);
            Assert.AreEqual(InputPropertyType.Basic, UpdateView.InputProperties[3].Type);

            var select = UpdateView.InputProperties[3].Value as MultiSelectList;
            Assert.AreEqual("StringProperty", select.DataTextField);
            Assert.AreEqual("Id", select.DataValueField);
            Assert.AreEqual(3, select.Items.OfType<AnotherEntity>().Count());

            select = UpdateView.InputProperties[4].Value as SelectList;
            Assert.AreEqual("StringProperty", select.DataTextField);
            Assert.AreEqual("Id", select.DataValueField);
            Assert.AreEqual(4, select.Items.Count());

            select = UpdateView.InputProperties[7].Value as SelectList;
            Assert.AreEqual("StringProperty", select.DataTextField);
            Assert.AreEqual("Id", select.DataValueField);
            Assert.AreEqual(4, select.Items.Count());
        }
    }

    public class AndGivenViewVisitors : AndBuildingTheUpdateView
    {
        protected Mock<IUpdateViewVisitor> MockUpdateVisitor;

        public override void Arrange()
        {
            base.Arrange();

            MockUpdateVisitor = new Mock<IUpdateViewVisitor>();
        }

        protected void AssertCommon()
        {
            MockUpdateVisitor.Verify(v => v.Visit(It.IsAny<InputProperty>()), Times.Exactly(8));
        }
    }

    [TestClass]
    public class ForAModel : AndGivenViewVisitors
    {
        public override void Act()
        {
            UpdateView = Builder.BuildUpdateView<TestEntity>(new TestEntity { Id = 1 }, MockUpdateVisitor.Object); 
        }

        [TestMethod]
        public void ThenTheVisitorShouldVisitEveryInputPropertyOfTheModel()
        {
            AssertCommon();
        }
    }

    [TestClass]
    public class ForAnId : AndGivenViewVisitors
    {
        public override void Act()
        {
            UpdateView = Builder.BuildUpdateView<TestEntity>(1, MockUpdateVisitor.Object);
        }

        [TestMethod]
        public void ThenTheVisitorShouldVisitEveryInputPropertyOfTheModel()
        {
            AssertCommon();
        }
    }

    [TestClass]
    public class ForAnIdAndTheVisitorReturnsANullInputProperty : AndGivenViewVisitors
    {
        public override void Arrange()
        {
            base.Arrange();

            MockUpdateVisitor.Setup(v => v.Visit(It.Is<InputProperty>(p => p.Label == "Name *"))).Returns(new InputProperty());
            MockUpdateVisitor.Setup(v => v.Visit(It.Is<InputProperty>(p => p.Label == "RequiredEntity *"))).Returns(() => null);
        }

        public override void Act()
        {
            UpdateView = Builder.BuildUpdateView<TestEntity>(new TestEntity { Name = "One", RequiredEntity = new AnotherEntity() }, 
                MockUpdateVisitor.Object);
        }

        [TestMethod]
        public void ThenTheNulledPropertyShouldNotBeInTheRenderedUpdateView()
        {
            AssertCommon();

            Assert.AreEqual(1, UpdateView.InputProperties.Count);
        }
    }

    #endregion
}

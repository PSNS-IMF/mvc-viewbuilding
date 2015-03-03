using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Reflection;

using System.Globalization;
using System.Data.Entity.Design.PluralizationServices;
using System.ComponentModel.DataAnnotations;

using Psns.Common.Persistence.Definitions;

using Psns.Common.Mvc.ViewBuilding.Entities;
using Psns.Common.Mvc.ViewBuilding.Attributes;
using Psns.Common.Mvc.ViewBuilding.ViewModels;
using Psns.Common.Mvc.ViewBuilding.ViewModels.TableModel;
using Psns.Common.Mvc.ViewBuilding.Adapters;

using System.Web.Mvc;

namespace Psns.Common.Mvc.ViewBuilding.ViewBuilders
{
    public interface ICrudViewBuilder
    {
        IndexView BuildIndexView<T>(int? page = null,
            int? pageSize = null,
            string sortKey = null,
            string sortDirection = null,
            IEnumerable<string> filterKeys = null,
            IEnumerable<string> filterValues = null,
            string searchQuery = null,
            params IIndexViewVisitor[] viewVisitors) where T : class, IIdentifiable;

        DetailsView BuildDetailsView<T>(int id, params IDetailsViewVisitor[] viewVisitors) where T : class, IIdentifiable, INameable;
        UpdateView BuildUpdateView<T>(int? id) where T : class, IIdentifiable, INameable;
        UpdateView BuildUpdateView<T>(T model) where T : class, IIdentifiable, INameable;

        IEnumerable<FilterOption> GetIndexFilterOptions<T>() where T : class, IIdentifiable;
    }

    public class CrudViewBuilder : ICrudViewBuilder
    {
        IRepositoryFactory _repositoryFactory;
        PluralizationService _pluralizer;
        CultureInfo _cultureInfo;

        public CrudViewBuilder(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _cultureInfo = CultureInfo.GetCultureInfo("en-US");
            _pluralizer = PluralizationService.CreateService(_cultureInfo);
        }

        public IndexView BuildIndexView<T>(int? page = null,
            int? pageSize = null,
            string sortKey = null,
            string sortDirection = null,
            IEnumerable<string> filterKeys = null,
            IEnumerable<string> filterValues = null,
            string searchQuery = null,
            params IIndexViewVisitor[] viewVisitors) where T : class, IIdentifiable
        {
            viewVisitors = viewVisitors ?? new IIndexViewVisitor[0];
            page = page ?? 1;
            pageSize = pageSize ?? 25;

            Type modelType = typeof(T);
            string modelName = modelType.Name;

            var view = new IndexView(modelType.AssemblyQualifiedName) 
            { 
                Title = _pluralizer.Pluralize(modelName)
            };

            var urlHelper = new UrlHelper(RequestContextAdapter.Context);

            BuildSearchControl(searchQuery, view, urlHelper);

            var indexProperties = GetIndexProperties<T>();

            AddHeaderRow(view.Table, indexProperties);

            var entities = _repositoryFactory.Get<T>().All(CrudEntityExtensions.GetComplexPropertyNames(indexProperties)); // determine includes

            entities = Search<T>(entities, searchQuery);
            Filter<T>(ref entities, filterKeys, filterValues, indexProperties);

            int pageCount = entities.Count() / pageSize.Value;

            BuildPager(page, pageSize, sortKey, sortDirection, filterKeys, filterValues, searchQuery, view, ref pageCount, urlHelper);
            Sort<T>(ref entities, sortKey, sortDirection, indexProperties);

            entities = entities.Skip((page.Value - 1)*pageSize.Value).Take(pageSize.Value);

            int rowCount = 0;
            foreach(var mappable in entities)
            {
                var row = new Row { Id = mappable.Id };

                if(++rowCount % 2 == 0)
                    row.Html["class"] = "pure-table-odd";

                row.Html["data-link"] = urlHelper.RouteUrl("Details", new 
                { 
                    controller = _pluralizer.Pluralize(modelName),
                    action = "Details",
                    id = mappable.Id 
                });

                foreach(var property in indexProperties)
                {
                    var value = GetPropertyValue<T>(mappable, property);

                    if(value == null)
                        value = string.Empty;

                    var column = new Column { Value = value };

                    foreach(var visitor in viewVisitors)
                        column.Accept(visitor);

                    row.Columns.Add(column);
                }

                if(row.Columns.Count > 0)
                {
                    foreach(var visitor in viewVisitors)
                        row.Accept(visitor);

                    view.Table.Rows.Add(row);
                }
            }

            foreach(var visitor in viewVisitors)
                view.Table.Accept(visitor);

            var create = new ActionModel
            {
                Type = ActionType.Link,
                RouteName = "Default",
                RouteValues = new AttributeDictionary(new
                {
                    controller = _pluralizer.Pluralize(modelName),
                    action = "Update"
                }),
                Html = new AttributeDictionary(new
                {
                    @class = "pure-button button-success",
                    title = "Create New " + modelName
                })
            };
            create.IconHtmlClasses.Add("fa fa-plus fa-lg");
            view.ContextItems.Add(create);

            foreach(var visitor in viewVisitors)
                view.Accept(visitor);

            return view;
        }

        public DetailsView BuildDetailsView<T>(int id, params IDetailsViewVisitor[] viewVisitors) where T : class, IIdentifiable, INameable
        {
            int orderCount = 0;
            var detailsProperties = typeof(T)
                .GetProperties()
                .Where(p => (p.GetCustomAttributes(typeof(ViewDisplayablePropertyAttribute), false) as ViewDisplayablePropertyAttribute[])
                            .Where(a => a.DisplayViewTypes.Contains(DisplayViewTypes.Details))
                            .Any())
                .OrderBy(p => p.GetPropertyOrder(orderCount));

            var includes = CrudEntityExtensions.GetComplexPropertyNames(detailsProperties);

            var model = _repositoryFactory.Get<T>().Find(e => e.Id == id, includes).SingleOrDefault();

            if(model == null)
            {
                throw new InvalidOperationException(string.Format("{0} with id {1} was not found",
                    typeof(T).Name,
                    id));
            }

            var view = new DetailsView
            {
                Title = model.Name
            };

            foreach(var property in detailsProperties)
            {
                var row = new Row();

                var titleColumn = new Column
                {
                    Value = GetPropertyName(property)
                };

                titleColumn.Html["style"] = "font-weight:bold;";

                var valueColumn = new Column
                {
                    Value = GetPropertyValue<T>(model, property)
                };

                foreach(var visitor in viewVisitors)
                {
                    visitor.Visit(titleColumn);
                    visitor.Visit(valueColumn);
                }

                row.Columns.Add(titleColumn);
                row.Columns.Add(valueColumn);

                foreach(var visitor in viewVisitors)
                    visitor.Visit(row);

                view.Table.Rows.Add(row);
            }

            foreach(var visitor in viewVisitors)
                visitor.Visit(view.Table);

            var up = new ActionModel
            {
                Type = ActionType.Link
            };

            var pluralName = _pluralizer.Pluralize(typeof(T).Name);

            up.Html["class"] = "pure-button pure-button-primary";
            up.Html["title"] = "Back to " + pluralName;
            up.IconHtmlClasses.Add("fa fa-level-up fa-lg");

            up.RouteName = "Default";
            up.RouteValues = new AttributeDictionary(new { controller = pluralName, action = "Index" });

            view.ContextItems.Add(up);

            var edit = new ActionModel
            {
                Type = ActionType.Link,
                RouteName = "Details",
                RouteValues = new AttributeDictionary(new
                {
                    controller = pluralName,
                    action = "Update",
                    id = model.Id
                }),
                Html = new AttributeDictionary(new
                {
                    @class = "pure-button",
                    title = "Update " + model.Name
                })
            };

            edit.IconHtmlClasses.Add("fa fa-edit fa-lg");

            view.ContextItems.Add(edit);

            var delete = new Form
            {
                FormMethod = FormMethod.Post,
                RouteName = "Details",
                RouteValues = new AttributeDictionary(new
                {
                    controller = pluralName,
                    action = "Delete",
                    id = model.Id
                }),
                Html = new AttributeDictionary(new
                {
                    @class = "delete",
                    style = "display: inline-block;"
                }),
                Submit = new ActionModel
                {
                    Type = ActionType.Button,
                    Html = new AttributeDictionary(new
                    {
                        @class = "pure-button button-warning",
                        title = "Delete " + model.Name,
                        type = "submit"
                    })
                }
            };

            delete.Submit.IconHtmlClasses.Add("fa fa-trash-o fa-lg");

            view.ContextItems.Add(delete);

            foreach(var visitor in viewVisitors)
                visitor.Visit(view);

            return view;
        }

        public UpdateView BuildUpdateView<T>(int? id) where T : class, IIdentifiable, INameable
        {
            T model;
            if(!id.HasValue)
                model = (T)Activator.CreateInstance(typeof(T));
            else
            {
                var includes = CrudEntityExtensions.GetComplexPropertyNames(typeof(T).GetUpdateProperties());
                model = _repositoryFactory.Get<T>().Find(e => e.Id == id, includes).SingleOrDefault();
            }

            if(model == null)
            {
                throw new InvalidOperationException(string.Format("{0} with id {1} was not found",
                    typeof(T).Name,
                    id));
            }

            return BuildUpdateView(model);
        }

        public UpdateView BuildUpdateView<T>(T model) where T : class, IIdentifiable, INameable
        {
            if(model == null)
                throw new InvalidOperationException("Model cannot be null");

            var modelType = typeof(T);
            var modelName = modelType.Name;
            var isUpdate = model.Id > 0;
            var pluralName = _pluralizer.Pluralize(modelName);

            var view = new UpdateView
            {
                Title = isUpdate 
                    ? (model.Name ?? modelName)
                    : string.Format("Create new {0}", modelName),
                Form = new Routeable
                {
                    Html = new AttributeDictionary(new { @class = "pure-form pure-form-aligned" }),
                    RouteName = isUpdate ? "Details" : "Default",
                    RouteValues = new AttributeDictionary(new 
                    { 
                        controller = pluralName, 
                        action = "Update",
                    }),
                    FormMethod = FormMethod.Post
                }
            };

            if(isUpdate)
                view.Form.RouteValues["id"] = model.Id;

            var saveButton = new ActionModel
            {
                Type = ActionType.Button,
                Html = new AttributeDictionary(new
                {
                    id = "saveButton",
                    @class = "pure-button pure-button-primary",
                    title = "Save"
                })
            };

            saveButton.IconHtmlClasses.Add("fa fa-floppy-o fa-lg");

            view.ContextItems.Add(saveButton);

            var cancelButton = new ActionModel
             {
                 Type = ActionType.Link,
                 Html = new AttributeDictionary(new
                 {
                     @class = "pure-button",
                     title = "Cancel"
                 }),
                 RouteName = "Default",
                 RouteValues = new AttributeDictionary(new
                 {
                     controller = pluralName,
                     action = "Index"
                 })
             };
 
            cancelButton.IconHtmlClasses.Add("fa fa-times fa-lg");

            view.ContextItems.Add(cancelButton);

            var updateProperties = typeof(T).GetUpdateProperties();

            foreach(var propertyInfo in updateProperties)
            {
                var updatableAttribute = (propertyInfo.GetCustomAttributes(typeof(ViewUpdatablePropertyAttribute), false) 
                    as IEnumerable<ViewUpdatablePropertyAttribute>).Single();
                var required = (propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), false) as RequiredAttribute[]).SingleOrDefault();
                var labelText = GetPropertyName(propertyInfo);
                if(required != null)
                    labelText += " *";

                var inputProperty = new InputProperty
                {
                    Label = labelText,
                    Type = updatableAttribute.InputPropertyType,
                    ModelName = propertyInfo.Name
                };

                var value = propertyInfo.GetValue(model, null);

                var complexAttribute = (propertyInfo.GetCustomAttributes(typeof(ViewComplexPropertyAttribute), false)
                    as IEnumerable<ViewComplexPropertyAttribute>).SingleOrDefault();

                if (complexAttribute != null) 
                {
                    var repositoryGetMethod = _repositoryFactory.GetType().GetMethod("Get");

                    if (propertyInfo.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)) &&
                        propertyInfo.PropertyType.IsGenericType) {
                        var genericGetMethod = repositoryGetMethod.MakeGenericMethod(propertyInfo.PropertyType.GetGenericArguments()[0]);
                        var repository = genericGetMethod.Invoke(_repositoryFactory, null);
                        var selections = repository.GetType().GetMethod("All").Invoke(repository, new object[] { null }) as IEnumerable;
                        var selectedValues = value != null
                            ? (value as IEnumerable).OfType<object>().Select(o =>
                                o.GetType()
                                .GetProperty(complexAttribute.ValuePropertyName)
                                .GetValue(o, null))
                            : null;

                        inputProperty.Value = new MultiSelectList(selections,
                            complexAttribute.ValuePropertyName,
                            complexAttribute.LabelPropertyName,
                            selectedValues);
                    }
                    else 
                    {
                        var complexPropertyInfo = propertyInfo;
                        var foreignKeyAttribute = (propertyInfo.GetCustomAttributes(typeof(ViewComplexPropertyForeignKeyAttribute), false)
                            as ViewComplexPropertyForeignKeyAttribute[]).SingleOrDefault();

                        if (foreignKeyAttribute != null) 
                        {
                            complexPropertyInfo = model.GetType().GetProperty(foreignKeyAttribute.ForPropertyName);
                            value = complexPropertyInfo.GetValue(model, null);
                        }

                        var genericGetMethod = repositoryGetMethod.MakeGenericMethod(complexPropertyInfo.PropertyType);
                        var repository = genericGetMethod.Invoke(_repositoryFactory, null);
                        var selections = repository.GetType().GetMethod("All").Invoke(repository, new object[] { null }) as IEnumerable;
                        var selectedValues = value != null
                            ? value.GetType().GetProperty(complexAttribute.ValuePropertyName).GetValue(value, null)
                            : null;

                        if (required == null)
                            selections = new List<object> { null }.Union(selections as IEnumerable<object>);

                        inputProperty.Value = new SelectList(selections,
                            complexAttribute.ValuePropertyName,
                            complexAttribute.LabelPropertyName,
                            selectedValues);
                    }
                }
                else 
                {
                    inputProperty.Value = value;
                }

                view.InputProperties.Add(inputProperty);
            }

            return view;
        }

        public IEnumerable<FilterOption> GetIndexFilterOptions<T>() where T : class, IIdentifiable
        {
            var columnNameOptionsMap = new Dictionary<string, List<string>>();
            var indexProperties = GetIndexProperties<T>();

            foreach(var item in _repositoryFactory.Get<T>().All(CrudEntityExtensions.GetComplexPropertyNames(indexProperties)))
            {
                foreach(var property in indexProperties)
                {
                    string columnName = GetPropertyName(property);

                    if(!columnNameOptionsMap.ContainsKey(columnName))
                        columnNameOptionsMap[columnName] = new List<string>();

                    var value = GetPropertyValue<T>(item, property);

                    if(value != null)
                        columnNameOptionsMap[columnName].Add(GetPropertyValue<T>(item, property).ToString());
                }
            }

            var filterOptions = new List<FilterOption>();

            foreach(var nameOptionPair in columnNameOptionsMap)
            {
                var option = new FilterOption { Label = nameOptionPair.Key };

                var children = new List<FilterOption>();
                foreach(var label in nameOptionPair.Value)
                    children.Add(new FilterOption { Label = label });

                option.Children = children;

                filterOptions.Add(option);
            }

            return filterOptions;
        }

        private static void BuildSearchControl(string searchQuery, IndexView view, UrlHelper urlHelper)
        {
            view.SearchControl.Query = searchQuery;

            view.SearchControl.InputHtml["class"] = "pure-input";
            view.SearchControl.InputHtml["style"] = "font-size: 100%;";
            view.SearchControl.InputHtml["name"] = "searchQuery";
            view.SearchControl.InputHtml["type"] = "text";

            view.SearchControl.Button.IconHtmlClasses.Add("fa fa-search");
            view.SearchControl.Button.Type = ActionType.Button;
            view.SearchControl.Button.Html["id"] = "searchButton";
            view.SearchControl.Button.Html["class"] = "pure-button pure-button-primary";
            view.SearchControl.Button.Html["data-link"] = urlHelper.RouteUrl("Default", new
            {
                controller = "IndexView",
                action = "RefreshTableBody"
            });
        }

        private static void BuildPager(int? page,
            int? pageSize,
            string sortKey,
            string sortDirection,
            IEnumerable<string> filterKeys,
            IEnumerable<string> filterValues,
            string searchQuery,
            IndexView view,
            ref int pageCount,
            UrlHelper urlHelper)
        {
            view.Pager.Html["class"] = "pure-paginator context-element";
            view.Pager.Html["style"] = "position:absolute;right:0px;";

            var refreshRouteValues = new AttributeDictionary(new
            {
                controller = "IndexView",
                action = "RefreshTableBody",
                page = page - 1
            });

            if(!string.IsNullOrEmpty(sortKey) && !string.IsNullOrEmpty(sortDirection))
            {
                refreshRouteValues["sortKey"] = sortKey;
                refreshRouteValues["sortDirection"] = sortDirection;
            }

            if((filterKeys != null && filterValues != null) &&
                filterKeys.Count() > 0 && filterValues.Count() > 0)
            {
                for(int i = 0; i < filterKeys.Count(); i++)
                {
                    refreshRouteValues["filterKeys[" + i + "]"] = filterKeys.ElementAt(i);
                    refreshRouteValues["filterValues[" + i + "]"] = filterValues.ElementAt(i);
                }

                if(filterValues.Count() > pageSize)
                    pageCount += filterValues.Count() / pageSize.Value;
                else if(filterValues.Count() == pageSize)
                    pageCount = filterValues.Count() / pageSize.Value;
            }

            if(!string.IsNullOrEmpty(searchQuery))
            {
                refreshRouteValues["searchQuery"] = searchQuery;
            }

            if(page > 1 && pageCount > 1)
            {
                view.Pager.Previous = new ActionModel
                {
                    Type = ActionType.Button,
                    Html = new AttributeDictionary(new
                    {
                        @class = "pure-button prev",
                        title = string.Format("Go to page {0}", page - 1)
                    })
                };

                view.Pager.Previous.Html["data-link"] = urlHelper.RouteUrl("Default", refreshRouteValues);

                view.Pager.Previous.IconHtmlClasses.Add("fa fa-angle-left fa-lg");
            }

            if(page > 2)
            {
                view.Pager.Previous.Html["class"] = "pure-button";

                view.Pager.First = new ActionModel
                {
                    Type = ActionType.Button,
                    Html = new AttributeDictionary(new
                    {
                        @class = "pure-button prev",
                        title = string.Format("Go to page {0}", 1)
                    })
                };

                refreshRouteValues["page"] = 1;
                view.Pager.First.Html["data-link"] = urlHelper.RouteUrl("Default", refreshRouteValues);

                view.Pager.First.IconHtmlClasses.Add("fa fa-angle-double-left fa-lg");
            }

            if(pageCount > 1)
            {
                view.Pager.PagerState = new ActionModel
                {
                    Type = ActionType.Button,
                    Text = string.Format("{0} of {1}", page, pageCount),
                    Html = new AttributeDictionary(new
                    {
                        @class = "pure-button pure-button-disabled"
                    })
                };
            }

            if(pageCount > 1 && page < pageCount)
            {
                view.Pager.Next = new ActionModel
                {
                    Type = ActionType.Button,
                    Html = new AttributeDictionary(new
                    {
                        @class = "pure-button next",
                        title = string.Format("Go to page {0}", page + 1)
                    })
                };

                refreshRouteValues["page"] = page + 1;
                view.Pager.Next.Html["data-link"] = urlHelper.RouteUrl("Default", refreshRouteValues);

                view.Pager.Next.IconHtmlClasses.Add("fa fa-angle-right fa-lg");
            }

            if(pageCount > 1 && page < pageCount - 1)
            {
                view.Pager.Next.Html["class"] = "pure-button";

                view.Pager.Last = new ActionModel
                {
                    Type = ActionType.Button,
                    Html = new AttributeDictionary(new
                    {
                        @class = "pure-button next",
                        title = string.Format("Go to page {0}", pageCount)
                    })
                };

                refreshRouteValues["page"] = pageCount;
                view.Pager.Last.Html["data-link"] = urlHelper.RouteUrl("Default", refreshRouteValues);

                view.Pager.Last.IconHtmlClasses.Add("fa fa-angle-double-right fa-lg");
            }
        }

        private static void AddHeaderRow(Table table, IOrderedEnumerable<PropertyInfo> indexProperties)
        {
            foreach(var property in indexProperties)
            {
                table.Header.Columns.Add(new Column { Value = GetPropertyName(property) });
            }
        }

        private static string GetPropertyName(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttributes(typeof(DisplayAttribute), false).SingleOrDefault();

            if(displayAttribute != null)
                return (displayAttribute as DisplayAttribute).Name ?? property.Name;
            else
                return property.Name;
        }

        private object GetPropertyValue<T>(T mappable, PropertyInfo property) where T : class, IIdentifiable
        {
            var value = property.GetValue(mappable, null);

            var complexAttributes = property.GetCustomAttributes(typeof(ViewComplexPropertyAttribute), false) as ViewComplexPropertyAttribute[];
            if(complexAttributes.Length > 0)
            {
                if(value is IEnumerable && value.GetType().IsGenericType)
                {
                    var collection = value as IEnumerable<object>;
                    if(collection.Count() > 0)
                    {
                        var columnValue = collection.Aggregate<object, string>("", (string sum, object item) =>
                        {
                            var section = item.GetType()
                                .GetProperty(complexAttributes[0].LabelPropertyName)
                                .GetValue(item, null)
                                .ToString();

                            if(item != collection.Last())
                            {
                                section += ", ";
                            }

                            return sum += section;
                        });

                        value = columnValue;
                    }
                    else
                        value = null;
                }
                else if(value != null)
                {
                    var foreignKeyAttribute = (property.GetCustomAttributes(typeof(ViewComplexPropertyForeignKeyAttribute), false) 
                        as ViewComplexPropertyForeignKeyAttribute[]).SingleOrDefault();

                    if(foreignKeyAttribute != null)
                    {
                        var fkPropertyValue = mappable.GetType().GetProperty(foreignKeyAttribute.ForPropertyName).GetValue(mappable, null);
                        value = fkPropertyValue.GetType().GetProperty(complexAttributes[0].LabelPropertyName).GetValue(fkPropertyValue, null);
                    }
                    else
                        value = value.GetType().GetProperty(complexAttributes[0].LabelPropertyName).GetValue(value, null);
                }
                else
                    value = null;
            }

            return value;
        }

        private IOrderedEnumerable<PropertyInfo> GetIndexProperties<T>() where T : IIdentifiable
        {
            int displayOrderCount = 0;

            var indexProperties = typeof(T)
                .GetProperties()
                .Where(p => (p.GetCustomAttributes(typeof(ViewDisplayablePropertyAttribute), false) as ViewDisplayablePropertyAttribute[])
                            .Where(a => a.DisplayViewTypes.Contains(DisplayViewTypes.Index))
                            .Any())
                .OrderBy(p => p.GetPropertyOrder(displayOrderCount));

            return indexProperties;
        }

        private IEnumerable<T> Search<T>(IEnumerable<T> entities, string searchQuery)
        {
            if(string.IsNullOrEmpty(searchQuery))
                return entities;

            var capitalSearch = searchQuery.Trim().ToUpper();
            var results = new List<T>() as IEnumerable<T>;

            foreach(var property in typeof(T).GetProperties())
            {
                results = results.Union(entities.Where(e => (property.GetValue(e, null) ?? "")
                    .ToString()
                    .ToUpper()
                    .Contains(capitalSearch)));
            }

            return results;
        }

        private void Sort<T>(ref IEnumerable<T> entities,
            string sortKey, 
            string sortDirection, 
            IOrderedEnumerable<PropertyInfo> indexProperties) where T : class, IIdentifiable
        {
            if(!string.IsNullOrEmpty(sortKey))
            {
                var sortPropertyInfo = GetMatchingProperty<T>(sortKey, indexProperties);

                if(string.IsNullOrEmpty(sortDirection) || sortDirection.ToUpper().Equals("ASC"))
                    entities = entities.OrderBy(property => GetPropertyValue<T>(property, sortPropertyInfo));
                else if(sortDirection.ToUpper().Equals("DESC"))
                    entities = entities.OrderByDescending(property => GetPropertyValue<T>(property, sortPropertyInfo));
                else
                    throw new InvalidOperationException(sortDirection + " is not a valid sort direction: should be asc, desc, or null.");
            }
        }

        private void Filter<T>(ref IEnumerable<T> entities,
            IEnumerable<string> filterKeys,
            IEnumerable<string> filterValues,
            IOrderedEnumerable<PropertyInfo> indexProperties) where T : class, IIdentifiable
        {
            if(filterKeys != null)
            {
                if(filterValues == null || filterKeys.Count() != filterValues.Count())
                    throw new InvalidOperationException("FilterKeys and FilterValues must contain the same number of items");

                var filterKeyValueMap = new Dictionary<string, IEnumerable<T>>();
                for(int i = 0; i < filterKeys.Count(); i++)
                {
                    var filterValue = filterValues.ElementAt(i);
                    if(!string.IsNullOrEmpty(filterValue))
                    {
                        var keyName = filterKeys.ElementAt(i);

                        if(!filterKeyValueMap.ContainsKey(keyName))
                            filterKeyValueMap[keyName] = new List<T>();

                        var keyProperty = GetMatchingProperty<T>(keyName, indexProperties);

                        filterKeyValueMap[keyName] = filterKeyValueMap[keyName].Union(entities.Where(e =>
                        {
                            var value = GetPropertyValue<T>(e, keyProperty);
                            return value != null ? value.ToString().Equals(filterValue) : false;
                        }));
                    }
                }

                foreach(var set in filterKeyValueMap)
                    entities = entities.Intersect(set.Value);
            }
        }

        private PropertyInfo GetMatchingProperty<T>(string name, 
            IOrderedEnumerable<PropertyInfo> indexProperties) where T : IIdentifiable
        {
            var sortPropertyName = _cultureInfo.TextInfo.ToTitleCase(name);
            var type = typeof(T);
            var sortPropertyInfo = typeof(T).GetProperty(sortPropertyName, 
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if(sortPropertyInfo == null)
            {
                var matchingProperties = indexProperties.Where(property =>
                {
                    var displayAttributes = property.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
                    if(displayAttributes.Length < 1)
                        return false;

                    return displayAttributes.Where(display => !string.IsNullOrEmpty(display.Name) &&
                        display.Name.Equals(sortPropertyName)).Count() > 0;
                });

                if(matchingProperties.Count() > 0)
                    sortPropertyInfo = matchingProperties.First();
                else
                    throw new InvalidOperationException(string.Format("{0} is not a valid property of {1}",
                        sortPropertyName,
                        type.Name));
            }
            return sortPropertyInfo;
        }
    }
}
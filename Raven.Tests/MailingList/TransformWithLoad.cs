﻿using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Tests.MailingList
{
    public class TransformWithLoad : RavenTestBase
    {
	    private EmbeddableDocumentStore store;

	    public TransformWithLoad()
	    {
		    store = NewDocumentStore();
			new ContactTransformer().Execute(store);
			new Contact_ByName().Execute(store);

		    using (var session = store.OpenSession())
		    {
			    var detail1 = new Detail
			    {
				    Name = "Detail 1",
			    };
			    var detail2 = new Detail
			    {
				    Name = "Detail 2"
			    };
			    session.Store(detail1);
			    session.Store(detail2);
			    session.SaveChanges();


			    var contact = new Contact
			    {
					Id = "contacts/1",
				    Name = "Contact 1",
				    DetailIds = new List<string>
				    {
					    detail1.Id,
					    detail2.Id
				    }
			    };
			    session.Store(contact);
			    session.SaveChanges();
		    }
	    }

        [Fact]
        public void Should_get_id_when_transformer_loads_document()
        {
	        using (var session = store.OpenSession())
	        {
		        // Act
		        var contactListViewModel = session.Query<Contact, Contact_ByName>().TransformWith<ContactTransformer, ContactDto>().ToList();

		        // Assert
		        foreach (var detail in contactListViewModel.SelectMany(c => c.ContactDetails))
		        {
			        Assert.NotNull(detail.Id);
		        }

		        var contactViewModel = session.Load<ContactTransformer, ContactDto>("contacts/1");
		        foreach (var detail in contactViewModel.ContactDetails)
		        {
			        Assert.NotNull(detail.Id);
		        }
	        }
        }

		[Fact]
		public void LazyLoadById()
		{
			using (var session = store.OpenSession())
			{
				var contactViewModel = session.Advanced.Lazily.Load<ContactTransformer, ContactDto>("contacts/1");
				var contactDto = contactViewModel.Value;
				foreach (var detail in contactDto.ContactDetails)
				{
					Assert.NotNull(detail.Id);
				}
			}
		}

		[Fact(Skip = "Currently not working")]
		public void LazyLoadByIds()
		{
			using (var session = store.OpenSession())
			{
				var contactViewModel = session.Advanced.Lazily.Load<ContactTransformer, ContactDto>("contacts/1", "contacts/2");
				var result = contactViewModel.Value;
				Assert.NotNull(result[0]);
				Assert.Null(result[1]);

				foreach (var detail in result[0].ContactDetails)
				{
					Assert.NotNull(detail.Id);
				}
			}
		}

	    public class Contact
        {
            public Contact()
            {
                DetailIds = new List<string>();
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public List<string> DetailIds { get; set; }
        }

        public class ContactDto
        {
            public string ContactId { get; set; }
            public string ContactName { get; set; }
            public List<Detail> ContactDetails { get; set; }
        }

        public class ContactTransformer : AbstractTransformerCreationTask<Contact>
        {
            public ContactTransformer()
            {
	            TransformResults = contacts => from c in contacts
	                                           select new
	                                           {
		                                           ContactId = c.Id,
		                                           ContactName = c.Name,
		                                           ContactDetails = LoadDocument<Detail>(c.DetailIds)
	                                           };
            }
        }

        public class Contact_ByName : AbstractIndexCreationTask<Contact>
        {
            public Contact_ByName()
            {
                Map = contacts => from c in contacts select new { c.Name };

                Index(x => x.Name, FieldIndexing.Analyzed);
            }
        }

        public class Detail
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GoCardless.Internals;
using GoCardless.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GoCardless.Services
{
    /// <summary>
    /// Service class for working with customer resources.
    ///
    /// Customer objects hold the contact details for a customer. A customer can
    /// have several [customer bank
    /// accounts](#core-endpoints-customer-bank-accounts), which in turn can
    /// have several Direct Debit [mandates](#core-endpoints-mandates).
    ///
    /// 
    /// Note: the `swedish_identity_number` field may only be supplied
    /// for Swedish customers, and must be supplied if you intend to set up an
    /// Autogiro mandate with the customer.
    /// </summary>

    public class CustomerService
    {
        private readonly GoCardlessClient _goCardlessClient;

        /// <summary>
        /// Constructor. Users of this library should not call this. An instance of this
        /// class can be accessed through an initialised GoCardlessClient.
        /// </summary>
        public CustomerService(GoCardlessClient goCardlessClient)
        {
            _goCardlessClient = goCardlessClient;
        }

        /// <summary>
        /// Creates a new customer object.
        /// </summary>
        /// <returns>A single customer resource</returns>
        public Task<CustomerResponse> CreateAsync(CustomerCreateRequest request = null, RequestSettings customiseRequestMessage = null)
        {
            request = request ?? new CustomerCreateRequest();

            var urlParams = new List<KeyValuePair<string, object>>
            {};

            return _goCardlessClient.ExecuteAsync<CustomerResponse>("POST", "/customers", urlParams, request, id => GetAsync(id, null, customiseRequestMessage), "customers", customiseRequestMessage);
        }

        /// <summary>
        /// Returns a [cursor-paginated](#api-usage-cursor-pagination) list of
        /// your customers.
        /// </summary>
        /// <returns>A set of customer resources</returns>
        public Task<CustomerListResponse> ListAsync(CustomerListRequest request = null, RequestSettings customiseRequestMessage = null)
        {
            request = request ?? new CustomerListRequest();

            var urlParams = new List<KeyValuePair<string, object>>
            {};

            return _goCardlessClient.ExecuteAsync<CustomerListResponse>("GET", "/customers", urlParams, request, null, null, customiseRequestMessage);
        }

        /// <summary>
        /// Get a lazily enumerated list of customers.
        /// This acts like the #list method, but paginates for you automatically.
        /// </summary>
        public IEnumerable<Customer> All(CustomerListRequest request = null, RequestSettings customiseRequestMessage = null)
        {
            request = request ?? new CustomerListRequest();

            string cursor = null;
            do
            {
                request.After = cursor;

                var result = Task.Run(() => ListAsync(request, customiseRequestMessage)).Result;
                foreach (var item in result.Customers)
                {
                    yield return item;
                }
                cursor = result.Meta?.Cursors?.After;
            } while (cursor != null);
        }

        /// <summary>
        /// Get a lazily enumerated list of customers.
        /// This acts like the #list method, but paginates for you automatically.
        /// </summary>
        public IEnumerable<Task<IReadOnlyList<Customer>>> AllAsync(CustomerListRequest request = null, RequestSettings customiseRequestMessage = null)
        {
            request = request ?? new CustomerListRequest();

            return new TaskEnumerable<IReadOnlyList<Customer>, string>(async after =>
            {
                request.After = after;
                var list = await this.ListAsync(request, customiseRequestMessage);
                return Tuple.Create(list.Customers, list.Meta?.Cursors?.After);
            });
        }

        /// <summary>
        /// Retrieves the details of an existing customer.
        /// </summary>
        /// <param name="identity">Unique identifier, beginning with "CU".</param>
        /// <returns>A single customer resource</returns>
        public Task<CustomerResponse> GetAsync(string identity, CustomerGetRequest request = null, RequestSettings customiseRequestMessage = null)
        {
            request = request ?? new CustomerGetRequest();
            if (identity == null) throw new ArgumentException(nameof(identity));

            var urlParams = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("identity", identity),
            };

            return _goCardlessClient.ExecuteAsync<CustomerResponse>("GET", "/customers/:identity", urlParams, request, null, null, customiseRequestMessage);
        }

        /// <summary>
        /// Updates a customer object. Supports all of the fields supported when
        /// creating a customer.
        /// </summary>
        /// <param name="identity">Unique identifier, beginning with "CU".</param>
        /// <returns>A single customer resource</returns>
        public Task<CustomerResponse> UpdateAsync(string identity, CustomerUpdateRequest request = null, RequestSettings customiseRequestMessage = null)
        {
            request = request ?? new CustomerUpdateRequest();
            if (identity == null) throw new ArgumentException(nameof(identity));

            var urlParams = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("identity", identity),
            };

            return _goCardlessClient.ExecuteAsync<CustomerResponse>("PUT", "/customers/:identity", urlParams, request, null, "customers", customiseRequestMessage);
        }
    }

        
    public class CustomerCreateRequest : IHasIdempotencyKey
    {

        /// <summary>
        /// The first line of the customer's address.
        /// </summary>
        [JsonProperty("address_line1")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// The second line of the customer's address.
        /// </summary>
        [JsonProperty("address_line2")]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// The third line of the customer's address.
        /// </summary>
        [JsonProperty("address_line3")]
        public string AddressLine3 { get; set; }

        /// <summary>
        /// The city of the customer's address.
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// Customer's company name. Required unless a `given_name` and
        /// `family_name` are provided.
        /// </summary>
        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        /// <summary>
        /// [ISO
        /// 3166-1](http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2#Officially_assigned_code_elements)
        /// alpha-2 code.
        /// </summary>
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Customer's email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Customer's surname. Required unless a `company_name` is provided.
        /// </summary>
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

        /// <summary>
        /// Customer's first name. Required unless a `company_name` is provided.
        /// </summary>
        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        /// <summary>
        /// [ISO 639-1](http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes)
        /// code. Used as the language for notification emails sent by
        /// GoCardless if your organisation does not send its own (see
        /// [compliance requirements](#appendix-compliance-requirements)).
        /// Currently only "en", "fr", "de", "pt", "es", "it", "nl", "sv" are
        /// supported. If this is not provided, the language will be chosen
        /// based on the `country_code` (if supplied) or default to "en".
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Key-value store of custom data. Up to 3 keys are permitted, with key
        /// names up to 50 characters and values up to 500 characters.
        /// </summary>
        [JsonProperty("metadata")]
        public IDictionary<String, String> Metadata { get; set; }

        /// <summary>
        /// The customer's postal code.
        /// </summary>
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        /// <summary>
        /// The customer's address region, county or department.
        /// </summary>
        [JsonProperty("region")]
        public string Region { get; set; }

        /// <summary>
        /// For Swedish customers only. The civic/company number (personnummer,
        /// samordningsnummer, or organisationsnummer) of the customer. Must be
        /// supplied if the customer's bank account is denominated in Swedish
        /// krona (SEK). This field cannot be changed once it has been set. <p
        /// class="beta-notice"><strong>Beta</strong>: this field is only used
        /// for Autogiro, which is currently in beta.</p>
        /// </summary>
        [JsonProperty("swedish_identity_number")]
        public string SwedishIdentityNumber { get; set; }

        [JsonIgnore]
        public string IdempotencyKey { get; set; }
    }

        
    public class CustomerListRequest
    {

        /// <summary>
        /// Cursor pointing to the start of the desired set.
        /// </summary>
        [JsonProperty("after")]
        public string After { get; set; }

        /// <summary>
        /// Cursor pointing to the end of the desired set.
        /// </summary>
        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("created_at")]
        public CreatedAtParam CreatedAt { get; set; }

        public class CreatedAtParam
        {
            /// <summary>
            /// Limit to records created within certain times
            /// </summary>
            [JsonProperty("gt")]
            public DateTimeOffset? GreaterThan { get; set; }

            [JsonProperty("gte")]
            public DateTimeOffset? GreaterThanOrEqual { get; set; }

            [JsonProperty("lt")]
            public DateTimeOffset? LessThan { get; set; }

            [JsonProperty("lte")]
            public DateTimeOffset? LessThanOrEqual { get; set; }
        }

        /// <summary>
        /// Number of records to return.
        /// </summary>
        [JsonProperty("limit")]
        public int? Limit { get; set; }
    }

        
    public class CustomerGetRequest
    {
    }

        
    public class CustomerUpdateRequest
    {

        /// <summary>
        /// The first line of the customer's address.
        /// </summary>
        [JsonProperty("address_line1")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// The second line of the customer's address.
        /// </summary>
        [JsonProperty("address_line2")]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// The third line of the customer's address.
        /// </summary>
        [JsonProperty("address_line3")]
        public string AddressLine3 { get; set; }

        /// <summary>
        /// The city of the customer's address.
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// Customer's company name. Required unless a `given_name` and
        /// `family_name` are provided.
        /// </summary>
        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        /// <summary>
        /// [ISO
        /// 3166-1](http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2#Officially_assigned_code_elements)
        /// alpha-2 code.
        /// </summary>
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Customer's email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Customer's surname. Required unless a `company_name` is provided.
        /// </summary>
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

        /// <summary>
        /// Customer's first name. Required unless a `company_name` is provided.
        /// </summary>
        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        /// <summary>
        /// [ISO 639-1](http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes)
        /// code. Used as the language for notification emails sent by
        /// GoCardless if your organisation does not send its own (see
        /// [compliance requirements](#appendix-compliance-requirements)).
        /// Currently only "en", "fr", "de", "pt", "es", "it", "nl", "sv" are
        /// supported. If this is not provided, the language will be chosen
        /// based on the `country_code` (if supplied) or default to "en".
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Key-value store of custom data. Up to 3 keys are permitted, with key
        /// names up to 50 characters and values up to 500 characters.
        /// </summary>
        [JsonProperty("metadata")]
        public IDictionary<String, String> Metadata { get; set; }

        /// <summary>
        /// The customer's postal code.
        /// </summary>
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        /// <summary>
        /// The customer's address region, county or department.
        /// </summary>
        [JsonProperty("region")]
        public string Region { get; set; }

        /// <summary>
        /// For Swedish customers only. The civic/company number (personnummer,
        /// samordningsnummer, or organisationsnummer) of the customer. Must be
        /// supplied if the customer's bank account is denominated in Swedish
        /// krona (SEK). This field cannot be changed once it has been set. <p
        /// class="beta-notice"><strong>Beta</strong>: this field is only used
        /// for Autogiro, which is currently in beta.</p>
        /// </summary>
        [JsonProperty("swedish_identity_number")]
        public string SwedishIdentityNumber { get; set; }
    }

    public class CustomerResponse : ApiResponse
    {
        [JsonProperty("customers")]
        public Customer Customer { get; private set; }
    }

    public class CustomerListResponse : ApiResponse
    {
        public IReadOnlyList<Customer> Customers { get; private set; }
        public Meta Meta { get; private set; }
    }
}
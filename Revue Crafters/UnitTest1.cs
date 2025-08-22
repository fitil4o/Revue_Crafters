using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using Revue_Crafters.Models;
using System.Net;
using System.Text.Json;

namespace Revue_Crafters
{
    [TestFixture]
    public class RevueCraftersTests
    {
        private RestClient client;
        private static string lastCreatedRevueId;
        private const string BaseUrl = "https://d2925tksfvgq8c.cloudfront.net";

        private const string LoginEmail = "vasko85@vasko.com";
        private const string LoginPassword = "vasko85";


        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken( LoginEmail, LoginPassword );

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken( string email, string password)
        {
            var loginClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }
        [Test, Order(1)]

        public void CreateRevue_ShouldReturnCreatedRevue()
        {
            var newRevue = new
            {
                Title = "New Revue",
                Url = "",
                Description = "This is the new revue"
            };
            var request = new RestRequest("/api/Revue/Create", Method.Post);
            request.AddJsonBody(newRevue);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Test, Order(2)]
        public void GetAllRevue_ShouldReturnOkAndNotEmptyList()
        {
            var request = new RestRequest("/api/Revue/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Console.WriteLine(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);

            lastCreatedRevueId = responseItems.LastOrDefault()?.Id;
            Assert.That(lastCreatedRevueId, Is.Not.Null);
        }

        [Test, Order(3)]
        public void EditLastCreatedRevue_ShouldReturnOk()

        {
            var editRequest = new RevueDTO
            {
                Title = "Edited Revue",
                Url = "",
                Description = "This is the last edited revue"
            };
            var request = new RestRequest("/api/Revue/Edit", Method.Put);
            request.AddQueryParameter("revueId", lastCreatedRevueId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }

        [Test, Order(4)]

        public void DeleteLastCreatedRevue_ShouldReturnOk()
        {
            var request = new RestRequest("/api/Revue/Delete", Method.Delete);
            request.AddQueryParameter("revueId", lastCreatedRevueId);
            var response = this.client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteResponse.Msg, Is.EqualTo("The revue is deleted!"));
        }
        [Test, Order(5)]
        public void CreateRevueWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var newRevue = new
            {
                Url = "",
            };
            var request = new RestRequest("/api/Revue/Create", Method.Post);
            request.AddJsonBody(newRevue);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test, Order(6)]
        public void EditNonExistingRevue_ShouldReturnBadRequest()
        {
            string fakeId = "1234";
            var editRequest = new RevueDTO
            {
                Title = "Edited Revue",
                Url = "",
                Description = "This is the last edited revue"
            };
            var request = new RestRequest("/api/Revue/Edit", Method.Put);
            request.AddQueryParameter("revueId", fakeId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such revue"));
        }

        [Test, Order(7)]
        public void DeleteNonExistingRevue_ShouldReturnBadRequest()
        {
            string fakeId = "12345";
            var request = new RestRequest("/api/Revue/Delete", Method.Delete);
            request.AddQueryParameter("revueId", fakeId);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such revue"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}
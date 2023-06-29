using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AmidoTechUnitTests
{
    [TestFixture]

    public class UnitTests
    {


        private HttpClient httpClient;
        private string baseUrl;
        private string authToken;
        // private string userId;

        [SetUp]
        public async Task SetUp()
        {

            httpClient = new HttpClient();
            baseUrl = "https://amido-tech-test.herokuapp.com";

            var tokenResponse = await httpClient.GetStringAsync($"{baseUrl}/token");
            authToken = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(tokenResponse).Token;
        }

        [TearDown]
        public void TearDown()
        {
            httpClient.Dispose();
        }

        [Test]
        public async Task CreateUserTest()
        {

            var createUserDetails = "{\"name\":\"Joe\",\"password\":\"MyCurrentPassword\"}";
            var createUserJson = new StringContent(createUserDetails, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var createUserResponse = await httpClient.PostAsync($"{baseUrl}/users", createUserJson);
            string responseContent = await createUserResponse.Content.ReadAsStringAsync();

            Assert.That(createUserResponse.IsSuccessStatusCode, Is.True);
            Assert.That(responseContent, Contains.Substring("Created"), "User was not created");


            // Gets the response header location value
            var location = createUserResponse.Headers.Location;

            //get last element from a uri
            var userId = location.Segments[^1];

            Console.WriteLine("User has been created with id: " + userId);
        }

        [Test]
        public async Task GetUserTest()
        {
            //Part1 create the user and gets userId
            var createUserDetails = "{\"name\":\"Joe\",\"password\":\"MyCurrentPassword\"}";
            var createUserJson = new StringContent(createUserDetails, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var createUserResponse = await httpClient.PostAsync($"{baseUrl}/users", createUserJson);
            string responseContent = await createUserResponse.Content.ReadAsStringAsync();
            Assert.That(responseContent, Contains.Substring("Created"), "User was not created"); //check user is created
            var location = createUserResponse.Headers.Location;             // Gets the response header location value
            var userId = location.Segments[^1];                           //get last segment from a uri


            //Gets User api call
            var getUserResponse = await httpClient.GetAsync($"{baseUrl}/users/{userId}");
            Assert.That(getUserResponse.IsSuccessStatusCode, Is.True);

            // Assert or perform further actions with the returned userJson
            var userJson = await getUserResponse.Content.ReadAsStringAsync();
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserResponse>(userJson);
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Name, Is.EqualTo("Joe"));
            Assert.That(user.Password, Is.EqualTo("MyCurrentPassword"));
            ;
            Console.WriteLine("User Details: " + userJson);
        }


        //Writing following as unit tests  - as userId is fixed - will not perform create and get UserId as user already there.
        [Test]
        public async Task UpDateUserPasswordTest()
        {
            var userId = "59374fh3rhfjsjjjakskw8w";
            var updatePasswordDetails = "{\"name\":\"Joe\",\"password\":\"MyNewPassword\"}";
            var updatePasswordJson = new StringContent(updatePasswordDetails, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            //Update User Password
            var updatePasswordResponse = await httpClient.PutAsync($"{baseUrl}/users/{userId}", updatePasswordJson);
            Assert.That(updatePasswordResponse.IsSuccessStatusCode, Is.True);
            var userJson = await updatePasswordResponse.Content.ReadAsStringAsync();
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserResponse>(userJson);
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Password, Is.EqualTo("MyNewPassword"));
        }

        [Test]
        public async Task DeleteUserTest()
        {
            var userId = "59374fh3rhfjsjjjakskw8w";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            //DeleteUser
            var deleteUserResponse = await httpClient.DeleteAsync($"{baseUrl}/users/{userId}");
            Assert.That(deleteUserResponse.IsSuccessStatusCode, Is.True);
        }



        private class TokenResponse
        {
            public string Token { get; set; }
        }

        private class UserResponse
        {
            public string Name { get; set; }
            public string Password { get; set; }
            public Contact[] Contact { get; set; }
        }

        private class Contact
        {
            public string Tel { get; set; }
            public string Mob { get; set; }
        }
    }
}
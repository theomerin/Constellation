using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium.Environment;

namespace OpenQA.Selenium
{
    [TestFixture]
    public class CookieImplementationTest : DriverTestFixture
    {
        private Random random = new Random();
        private bool isOnAlternativeHostName;
        private string hostname;
  

        [SetUp]
        public void GoToSimplePageAndDeleteCookies()
        {
            GotoValidDomainAndClearCookies("animals");
            AssertNoCookiesArePresent();
        }

        [Test]
        [Category("JavaScript")]
        public void ShouldGetCookieByName()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string key = string.Format("key_{0}", new Random().Next());
            ((IJavaScriptExecutor)driver).ExecuteScript("document.cookie = arguments[0] + '=set';", key);

            Cookie cookie = driver.Manage().Cookies.GetCookieNamed(key);
            Assert.AreEqual("set", cookie.Value);
        }

        [Test]
        [Category("JavaScript")]
        public void ShouldBeAbleToAddCookie()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string key = GenerateUniqueKey();
            string value = "foo";
            Cookie cookie = new Cookie(key, value);
            AssertCookieIsNotPresentWithName(key);

            driver.Manage().Cookies.AddCookie(cookie);

            AssertCookieHasValue(key, value);
            Assert.That(driver.Manage().Cookies.AllCookies.Contains(cookie), "Cookie was not added successfully");
        }

        [Test]
        public void GetAllCookies()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string key1 = GenerateUniqueKey();
            string key2 = GenerateUniqueKey();

            AssertCookieIsNotPresentWithName(key1);
            AssertCookieIsNotPresentWithName(key2);
            
            ReadOnlyCollection<Cookie> cookies = driver.Manage().Cookies.AllCookies;
            int count = cookies.Count;

            Cookie one = new Cookie(key1, "value");
            Cookie two = new Cookie(key2, "value");

            driver.Manage().Cookies.AddCookie(one);
            driver.Manage().Cookies.AddCookie(two);

            driver.Url = simpleTestPage;
            cookies = driver.Manage().Cookies.AllCookies;
            Assert.AreEqual(count + 2, cookies.Count);

            Assert.IsTrue(cookies.Contains(one));
            Assert.IsTrue(cookies.Contains(two));
        }

        [Test]
        [Category("JavaScript")]
        public void DeleteAllCookies()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            ((IJavaScriptExecutor)driver).ExecuteScript("document.cookie = 'foo=set';");
            AssertSomeCookiesArePresent();

            driver.Manage().Cookies.DeleteAllCookies();

            AssertNoCookiesArePresent();
        }

        [Test]
        [Category("JavaScript")]
        public void DeleteCookieWithName()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string key1 = GenerateUniqueKey();
            string key2 = GenerateUniqueKey();

            ((IJavaScriptExecutor)driver).ExecuteScript("document.cookie = arguments[0] + '=set';", key1);
            ((IJavaScriptExecutor)driver).ExecuteScript("document.cookie = arguments[0] + '=set';", key2);

            AssertCookieIsPresentWithName(key1);
            AssertCookieIsPresentWithName(key2);

            driver.Manage().Cookies.DeleteCookieNamed(key1);

            AssertCookieIsNotPresentWithName(key1);
            AssertCookieIsPresentWithName(key2);
        }

        [Test]
        public void ShouldNotDeleteCookiesWithASimilarName()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string cookieOneName = "fish";
            Cookie cookie1 = new Cookie(cookieOneName, "cod");
            Cookie cookie2 = new Cookie(cookieOneName + "x", "earth");
            IOptions options = driver.Manage();
            AssertCookieIsNotPresentWithName(cookie1.Name);
  
            options.Cookies.AddCookie(cookie1);
            options.Cookies.AddCookie(cookie2);

            AssertCookieIsPresentWithName(cookie1.Name);
   
            options.Cookies.DeleteCookieNamed(cookieOneName);

            Assert.IsFalse(driver.Manage().Cookies.AllCookies.Contains(cookie1));
            Assert.IsTrue(driver.Manage().Cookies.AllCookies.Contains(cookie2));
        }

        [Test]
        public void AddCookiesWithDifferentPathsThatAreRelatedToOurs()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string basePath = EnvironmentManager.Instance.UrlBuilder.Path;

            Cookie cookie1 = new Cookie("fish", "cod", "/" + basePath + "/animals");
            Cookie cookie2 = new Cookie("planet", "earth", "/" + basePath + "/");
            IOptions options = driver.Manage();
            options.Cookies.AddCookie(cookie1);
            options.Cookies.AddCookie(cookie2);

            UrlBuilder builder = EnvironmentManager.Instance.UrlBuilder;
            driver.Url = builder.WhereIs("animals");

            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            AssertCookieIsPresentWithName(cookie1.Name);
            AssertCookieIsPresentWithName(cookie2.Name);

            driver.Url = builder.WhereIs("");
            AssertCookieIsNotPresentWithName(cookie1.Name);
        }

        [Test]
        [IgnoreBrowser(Browser.Opera)]
        public void CannotGetCookiesWithPathDifferingOnlyInCase()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string cookieName = "fish";
            driver.Manage().Cookies.AddCookie(new Cookie(cookieName, "cod", "/Common/animals"));

            driver.Url = EnvironmentManager.Instance.UrlBuilder.WhereIs("animals");
            Assert.IsNull(driver.Manage().Cookies.GetCookieNamed(cookieName));
        }

        [Test]
        public void ShouldNotGetCookieOnDifferentDomain()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string cookieName = "fish";
            driver.Manage().Cookies.AddCookie(new Cookie(cookieName, "cod"));
            AssertCookieIsPresentWithName(cookieName);

            driver.Url = EnvironmentManager.Instance.UrlBuilder.WhereElseIs("simpleTest.html");

            AssertCookieIsNotPresentWithName(cookieName);
        }

        [Test]
        [Ignore("Cannot run without creating subdomains in test environment")]
        public void ShouldBeAbleToAddToADomainWhichIsRelatedToTheCurrentDomain()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            AssertCookieIsNotPresentWithName("name");

            Regex replaceRegex = new Regex(".*?\\.");
            string shorter = replaceRegex.Replace(this.hostname, "", 1);
            Cookie cookie = new Cookie("name", "value", shorter, "/", GetTimeInTheFuture());

            driver.Manage().Cookies.AddCookie(cookie);

            AssertCookieIsPresentWithName("name");
        }

        [Test]
        [Ignore("Cannot run without creating subdomains in test environment")]
        public void ShouldNotGetCookiesRelatedToCurrentDomainWithoutLeadingPeriod()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string cookieName = "name";
            AssertCookieIsNotPresentWithName(cookieName);

            Regex replaceRegex = new Regex(".*?\\.");
            string shorter = replaceRegex.Replace(this.hostname, ".", 1);
            Cookie cookie = new Cookie(cookieName, "value", shorter, "/", GetTimeInTheFuture());
            driver.Manage().Cookies.AddCookie(cookie);
            AssertCookieIsNotPresentWithName(cookieName);
        }

        [Test]
        public void ShouldBeAbleToIncludeLeadingPeriodInDomainName()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            // Cookies cannot be set on domain names with less than 2 dots, so
            // localhost is out. If we are in that boat, bail the test.
            string hostName = EnvironmentManager.Instance.UrlBuilder.HostName;
            string[] hostNameParts = hostName.Split(new char[] { '.' });
            if (hostNameParts.Length < 3)
            {
                Assert.Ignore("Skipping test: Cookies can only be set on fully-qualified domain names.");
            }

            AssertCookieIsNotPresentWithName("name");

            // Replace the first part of the name with a period
            Regex replaceRegex = new Regex(".*?\\.");
            string shorter = replaceRegex.Replace(this.hostname, ".");
            Cookie cookie = new Cookie("name", "value", shorter, "/", DateTime.Now.AddSeconds(100000));

            driver.Manage().Cookies.AddCookie(cookie);

            AssertCookieIsPresentWithName("name");
        }

        [Test]
        [IgnoreBrowser(Browser.IE, "IE cookies do not conform to RFC, so setting cookie on domain fails.")]
        [IgnoreBrowser(Browser.Chrome, "Test tries to set the domain explicitly to 'localhost', which is not allowed.")]
        public void ShouldBeAbleToSetDomainToTheCurrentDomain()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            Uri url = new Uri(driver.Url);
            String host = url.Host + ":" + url.Port.ToString();

            Cookie cookie1 = new Cookie("fish", "cod", host, "/", null);
            IOptions options = driver.Manage();
            options.Cookies.AddCookie(cookie1);

            driver.Url = javascriptPage;
            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            Assert.IsTrue(cookies.Contains(cookie1));
        }

        [Test]
        public void ShouldWalkThePathToDeleteACookie()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string basePath = EnvironmentManager.Instance.UrlBuilder.Path;

            Cookie cookie1 = new Cookie("fish", "cod");
            driver.Manage().Cookies.AddCookie(cookie1);
            int count = driver.Manage().Cookies.AllCookies.Count;

            driver.Url = childPage;
            Cookie cookie2 = new Cookie("rodent", "hamster", "/" + basePath + "/child");
            driver.Manage().Cookies.AddCookie(cookie2);
            count = driver.Manage().Cookies.AllCookies.Count;

            driver.Url = grandchildPage;
            Cookie cookie3 = new Cookie("dog", "dalmation", "/" + basePath + "/child/grandchild/");
            driver.Manage().Cookies.AddCookie(cookie3);
            count = driver.Manage().Cookies.AllCookies.Count;

            driver.Url = (EnvironmentManager.Instance.UrlBuilder.WhereIs("child/grandchild"));
            driver.Manage().Cookies.DeleteCookieNamed("rodent");
            count = driver.Manage().Cookies.AllCookies.Count;

            Assert.IsNull(driver.Manage().Cookies.GetCookieNamed("rodent"));

            ReadOnlyCollection<Cookie> cookies = driver.Manage().Cookies.AllCookies;
            Assert.AreEqual(2, cookies.Count);
            Assert.IsTrue(cookies.Contains(cookie1));
            Assert.IsTrue(cookies.Contains(cookie3));

            driver.Manage().Cookies.DeleteAllCookies();
            driver.Url = grandchildPage;
            AssertNoCookiesArePresent();
        }

        [Test]
        [IgnoreBrowser(Browser.IE, "IE cookies do not conform to RFC, so setting cookie on domain fails.")]
        [IgnoreBrowser(Browser.Chrome, "Test tries to set the domain explicitly to 'localhost', which is not allowed.")]
        public void ShouldIgnoreThePortNumberOfTheHostWhenSettingTheCookie()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            Uri uri = new Uri(driver.Url);
            string host = string.Format("{0}:{1}", uri.Host, uri.Port);
            string cookieName = "name";
            AssertCookieIsNotPresentWithName(cookieName);
            Cookie cookie = new Cookie(cookieName, "value", host, "/", null);
            driver.Manage().Cookies.AddCookie(cookie);
            AssertCookieIsPresentWithName(cookieName);
        }

        [Test]
        public void CookieEqualityAfterSetAndGet()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string url = EnvironmentManager.Instance.UrlBuilder.WhereElseIs("animals");

            driver.Url = url;
            driver.Manage().Cookies.DeleteAllCookies();

            DateTime time = DateTime.Now.AddDays(1);
            Cookie cookie1 = new Cookie("fish", "cod", null, "/common/animals", time);
            IOptions options = driver.Manage();
            options.Cookies.AddCookie(cookie1);

            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            Cookie retrievedCookie = null;
            foreach (Cookie tempCookie in cookies)
            {
                if (cookie1.Equals(tempCookie))
                {
                    retrievedCookie = tempCookie;
                    break;
                }
            }

            Assert.IsNotNull(retrievedCookie);
            //Cookie.equals only compares name, domain and path
            Assert.AreEqual(cookie1, retrievedCookie);
        }

        [Test]
        [IgnoreBrowser(Browser.Chrome, "Chrome and Selenium, which use JavaScript to retrieve cookies, cannot return expiry info;")]
        [IgnoreBrowser(Browser.IE, "IE does not return expiry info")]
        [IgnoreBrowser(Browser.Android, "Chrome and Selenium, which use JavaScript to retrieve cookies, cannot return expiry info;")]
        public void ShouldRetainCookieExpiry()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string url = EnvironmentManager.Instance.UrlBuilder.WhereElseIs("animals");

            driver.Url = url;
            driver.Manage().Cookies.DeleteAllCookies();

            // DateTime.Now contains milliseconds; the returned cookie expire date
            // will not. So we need to truncate the milliseconds.
            DateTime current = DateTime.Now;
            DateTime expireDate = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, current.Second, DateTimeKind.Local).AddDays(1);

            Cookie addCookie = new Cookie("fish", "cod", "/common/animals", expireDate);
            IOptions options = driver.Manage();
            options.Cookies.AddCookie(addCookie);

            Cookie retrieved = options.Cookies.GetCookieNamed("fish");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(addCookie.Expiry, retrieved.Expiry, "Cookies are not equal");
        }

        [Test]
        public void SettingACookieThatExpiredInThePast()
        {
            string url = EnvironmentManager.Instance.UrlBuilder.WhereElseIs("animals");

            driver.Url = url;
            driver.Manage().Cookies.DeleteAllCookies();

            DateTime expires = DateTime.Now.AddSeconds(-1000);
            Cookie cookie = new Cookie("expired", "yes", "/common/animals", expires);
            IOptions options = driver.Manage();
            options.Cookies.AddCookie(cookie);

            cookie = options.Cookies.GetCookieNamed("fish");
            Assert.IsNull(cookie, "Cookie expired before it was set, so nothing should be returned: " + cookie);
        }
        
        [Test]
        public void CanSetCookieWithoutOptionalFieldsSet()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string key = GenerateUniqueKey();
            string value = "foo";
            Cookie cookie = new Cookie(key, value);
            AssertCookieIsNotPresentWithName(key);

            driver.Manage().Cookies.AddCookie(cookie);

            AssertCookieHasValue(key, value);
        }

        //////////////////////////////////////////////
        // Tests unique to the .NET language bindings
        //////////////////////////////////////////////

        [Test]
        public void CanSetCookiesOnADifferentPathOfTheSameHost()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string basePath = EnvironmentManager.Instance.UrlBuilder.Path;
            Cookie cookie1 = new Cookie("fish", "cod", "/" + basePath + "/animals");
            Cookie cookie2 = new Cookie("planet", "earth", "/" + basePath + "/galaxy");

            IOptions options = driver.Manage();
            ReadOnlyCollection<Cookie> count = options.Cookies.AllCookies;

            options.Cookies.AddCookie(cookie1);
            options.Cookies.AddCookie(cookie2);

            string url = EnvironmentManager.Instance.UrlBuilder.WhereIs("animals");
            driver.Url = url;
            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;

            Assert.IsTrue(cookies.Contains(cookie1));
            Assert.IsFalse(cookies.Contains(cookie2));

            driver.Url = EnvironmentManager.Instance.UrlBuilder.WhereIs("galaxy");
            cookies = options.Cookies.AllCookies;
            Assert.IsFalse(cookies.Contains(cookie1));
            Assert.IsTrue(cookies.Contains(cookie2));
        }

        [Test]
        public void ShouldNotBeAbleToSetDomainToSomethingThatIsUnrelatedToTheCurrentDomain()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            Cookie cookie1 = new Cookie("fish", "cod");
            IOptions options = driver.Manage();
            options.Cookies.AddCookie(cookie1);

            string url = EnvironmentManager.Instance.UrlBuilder.WhereElseIs("simpleTest.html");
            driver.Url = url;

            Assert.IsNull(options.Cookies.GetCookieNamed("fish"));
        }

        [Test]
        public void GetCookieDoesNotRetriveBeyondCurrentDomain()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            Cookie cookie1 = new Cookie("fish", "cod");
            IOptions options = driver.Manage();
            options.Cookies.AddCookie(cookie1);

            String url = EnvironmentManager.Instance.UrlBuilder.WhereElseIs("");
            driver.Url = url;

            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            Assert.IsFalse(cookies.Contains(cookie1));
        }

        [Test]
        public void ShouldAddCookieToCurrentDomainAndPath()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            // Cookies cannot be set on domain names with less than 2 dots, so
            // localhost is out. If we are in that boat, bail the test.
            string hostName = EnvironmentManager.Instance.UrlBuilder.HostName;
            string[] hostNameParts = hostName.Split(new char[] { '.' });
            if (hostNameParts.Length < 3)
            {
                Assert.Ignore("Skipping test: Cookies can only be set on fully-qualified domain names.");
            }

            driver.Url = macbethPage;
            IOptions options = driver.Manage();
            Cookie cookie = new Cookie("Homer", "Simpson", this.hostname, "/" + EnvironmentManager.Instance.UrlBuilder.Path, null);
            options.Cookies.AddCookie(cookie);
            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            Assert.That(cookies.Contains(cookie), "Valid cookie was not returned");
        }

        [Test]
        [IgnoreBrowser(Browser.IE, "Add cookie to unrelated domain silently fails for IE.")]
        [IgnoreBrowser(Browser.Chrome, "Chrome returns incorrect error code when setting cookie")]
        [ExpectedException(typeof(WebDriverException))]
        public void ShouldNotShowCookieAddedToDifferentDomain()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            driver.Url = macbethPage;
            IOptions options = driver.Manage();
            Cookie cookie = new Cookie("Bart", "Simpson", EnvironmentManager.Instance.UrlBuilder.HostName + ".com", EnvironmentManager.Instance.UrlBuilder.Path, null);
            options.Cookies.AddCookie(cookie);
            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            Assert.IsFalse(cookies.Contains(cookie), "Invalid cookie was returned");
        }

        [Test]
        [IgnoreBrowser(Browser.Chrome, "Test tries to set the domain explicitly to 'localhost', which is not allowed.")]
        public void ShouldNotShowCookieAddedToDifferentPath()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            driver.Url = macbethPage;
            IOptions options = driver.Manage();
            Cookie cookie = new Cookie("Lisa", "Simpson", EnvironmentManager.Instance.UrlBuilder.HostName, "/" + EnvironmentManager.Instance.UrlBuilder.Path + "IDoNotExist", null);
            options.Cookies.AddCookie(cookie);
            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            Assert.IsFalse(cookies.Contains(cookie), "Invalid cookie was returned");
        }

        // TODO(JimEvans): Disabling this test for now. If your network is using
        // something like OpenDNS or Google DNS which you may be automatically
        // redirected to a search page, which will be a valid page and will allow a
        // cookie to be created. Need to investigate further.
        // [Test]
        // [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowExceptionWhenAddingCookieToNonExistingDomain()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            driver.Url = macbethPage;
            driver.Url = "doesnot.noireallyreallyreallydontexist.com";
            IOptions options = driver.Manage();
            Cookie cookie = new Cookie("question", "dunno");
            options.Cookies.AddCookie(cookie);
        }

        [Test]
        [IgnoreBrowser(Browser.Chrome, "Chrome and Selenium, which use JavaScript to retrieve cookies, cannot return expiry info;")]
        public void ShouldReturnNullBecauseCookieRetainsExpiry()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            string url = EnvironmentManager.Instance.UrlBuilder.WhereElseIs("animals");
            driver.Url = url;

            driver.Manage().Cookies.DeleteAllCookies();

            Cookie addCookie = new Cookie("fish", "cod", "/common/animals", DateTime.Now.AddHours(-1));
            IOptions options = driver.Manage();
            options.Cookies.AddCookie(addCookie);

            Cookie retrieved = options.Cookies.GetCookieNamed("fish");
            Assert.IsNull(retrieved);
        }

        [Test]
        public void ShouldAddCookieToCurrentDomain()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            driver.Url = macbethPage;
            IOptions options = driver.Manage();
            Cookie cookie = new Cookie("Marge", "Simpson", "/");
            options.Cookies.AddCookie(cookie);
            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            Assert.That(cookies.Contains(cookie), "Valid cookie was not returned");
        }

        [Test]
        public void ShouldDeleteCookie()
        {
            if (!CheckIsOnValidHostNameForCookieTests())
            {
                return;
            }

            driver.Url = macbethPage;
            IOptions options = driver.Manage();
            Cookie cookieToDelete = new Cookie("answer", "42");
            Cookie cookieToKeep = new Cookie("canIHaz", "Cheeseburguer");
            options.Cookies.AddCookie(cookieToDelete);
            options.Cookies.AddCookie(cookieToKeep);
            ReadOnlyCollection<Cookie> cookies = options.Cookies.AllCookies;
            options.Cookies.DeleteCookie(cookieToDelete);
            ReadOnlyCollection<Cookie> cookies2 = options.Cookies.AllCookies;
            Assert.IsFalse(cookies2.Contains(cookieToDelete), "Cookie was not deleted successfully");
            Assert.That(cookies2.Contains(cookieToKeep), "Valid cookie was not returned");
        }

        //////////////////////////////////////////////
        // Support functions
        //////////////////////////////////////////////

        private void GotoValidDomainAndClearCookies(string page)
        {
            this.hostname = null;
            String hostname = EnvironmentManager.Instance.UrlBuilder.HostName;
            if (IsValidHostNameForCookieTests(hostname))
            {
                this.isOnAlternativeHostName = false;
                this.hostname = hostname;
            }

            hostname = EnvironmentManager.Instance.UrlBuilder.AlternateHostName;
            if (this.hostname == null && IsValidHostNameForCookieTests(hostname))
            {
                this.isOnAlternativeHostName = true;
                this.hostname = hostname;
            }

            GoToPage(page);

            driver.Manage().Cookies.DeleteAllCookies();
        }

        private bool CheckIsOnValidHostNameForCookieTests()
        {
            bool correct = this.hostname != null && IsValidHostNameForCookieTests(this.hostname);
            if (!correct)
            {
                System.Console.WriteLine("Skipping test: unable to find domain name to use");
            }

            return correct;
        }

        private void GoToPage(String pageName)
        {
            driver.Url = this.isOnAlternativeHostName ? EnvironmentManager.Instance.UrlBuilder.WhereElseIs(pageName) : EnvironmentManager.Instance.UrlBuilder.WhereIs(pageName);
        }

        private void GoToOtherPage(String pageName)
        {
            driver.Url = this.isOnAlternativeHostName ? EnvironmentManager.Instance.UrlBuilder.WhereIs(pageName) : EnvironmentManager.Instance.UrlBuilder.WhereElseIs(pageName);
        }
        
        private bool IsValidHostNameForCookieTests(string hostname)
        {
            // TODO(JimEvan): Some coverage is better than none, so we
            // need to ignore the fact that localhost cookies are problematic.
            // Reenable this when we have a better solution per DanielWagnerHall.
            // return !IsIpv4Address(hostname) && "localhost" != hostname;
            return !IsIpv4Address(hostname);
        }

        private static bool IsIpv4Address(string addrString)
        {
            return Regex.IsMatch(addrString, "\\d{1,3}(?:\\.\\d{1,3}){3}");
        }

        private string GenerateUniqueKey()
        {
            return string.Format("key_{0}", random.Next());
        }

        private string GetDocumentCookieOrNull()
        {
            IJavaScriptExecutor jsDriver = driver as IJavaScriptExecutor;
            if (jsDriver == null)
            {
                return null;
            }
            try
            {
                return (string)jsDriver.ExecuteScript("return document.cookie");
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private void AssertNoCookiesArePresent()
        {
            Assert.IsTrue(driver.Manage().Cookies.AllCookies.Count == 0, "Cookies were not empty");
            string documentCookie = GetDocumentCookieOrNull();
            if (documentCookie != null)
            {
                Assert.AreEqual(string.Empty, documentCookie, "Cookies were not empty");
            }
        }

        private void AssertSomeCookiesArePresent()
        {
            Assert.IsFalse(driver.Manage().Cookies.AllCookies.Count == 0, "Cookies were empty");
            String documentCookie = GetDocumentCookieOrNull();
            if (documentCookie != null)
            {
                Assert.AreNotEqual(string.Empty, documentCookie, "Cookies were empty");
            }
        }

        private void AssertCookieIsNotPresentWithName(string key)
        {
            Assert.IsNull(driver.Manage().Cookies.GetCookieNamed(key), "Cookie was present with name " + key);
            string documentCookie = GetDocumentCookieOrNull();
            if (documentCookie != null)
            {
                Assert.IsFalse(documentCookie.Contains(key + "="), "Cookie was present with name " + key);
            }
        }

        private void AssertCookieIsPresentWithName(string key)
        {
            Assert.IsNotNull(driver.Manage().Cookies.GetCookieNamed(key), "Cookie was present with name " + key);
            string documentCookie = GetDocumentCookieOrNull();
            if (documentCookie != null)
            {
                Assert.IsTrue(documentCookie.Contains(key + "="), "Cookie was present with name " + key);
            }
        }

        private void AssertCookieHasValue(string key, string value)
        {
            Assert.AreEqual(value, driver.Manage().Cookies.GetCookieNamed(key).Value, "Cookie had wrong value");
            string documentCookie = GetDocumentCookieOrNull();
            if (documentCookie != null)
            {
                Assert.IsTrue(documentCookie.Contains(key + "=" + value), "Cookie was present with name " + key);
            }
        }

        private DateTime GetTimeInTheFuture()
        {
            return DateTime.Now.Add(TimeSpan.FromMilliseconds(100000));
        }
    }
}
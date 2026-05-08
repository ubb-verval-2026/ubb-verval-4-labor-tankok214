using System;
using System.Linq;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.IO;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class BlazeDemoTests
{
    private IWebDriver driver;
    private const string BlazeDemoURL = "https://blazedemo.com/";

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
            // Ignore errors if unable to close the browser
        }
    }

    [Test]
    public void BlazeDemo_MexicoCityToDublin_ShouldHaveAtLeastThreeFlights()
    {
        // Arrange
        driver.Navigate().GoToUrl(BlazeDemoURL);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        // Select Mexico City from the first dropdown
        var departureDropdown = driver.FindElement(By.XPath("//select[1]"));
        departureDropdown.SendKeys("Mexico City");

        // Select Dublin from the second dropdown
        var arrivalDropdown = driver.FindElement(By.XPath("//select[2]"));
        arrivalDropdown.SendKeys("Dublin");

        // Click Find Flights button
        var findFlightsButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@value='Find Flights']")));
        findFlightsButton.Click();

        // Wait for results table to appear with at least 3 rows
        wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath("//table//tbody//tr")));

        // Assert - Count the number of flight rows in the results table
        var flightRows = driver.FindElements(By.XPath("//table//tbody//tr"));
        flightRows.Count.Should().BeGreaterThanOrEqualTo(3, because: "there should be at least 3 flight options from Mexico City to Dublin");
    }

    [Test]
    public void BlazeDemo_MexicoCityToDublin_CheapFlightScreenshot()
    {
        // Arrange
        const double maxPrice = 300.0;
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string screenshotPath = Path.Combine(desktopPath, $"BlazeDemo_CheapFlight_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        driver.Navigate().GoToUrl(BlazeDemoURL);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        // Select Mexico City from the first dropdown
        var departureDropdown = driver.FindElement(By.XPath("//select[1]"));
        departureDropdown.SendKeys("Mexico City");

        // Select Dublin from the second dropdown
        var arrivalDropdown = driver.FindElement(By.XPath("//select[2]"));
        arrivalDropdown.SendKeys("Dublin");

        // Click Find Flights button
        var findFlightsButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@value='Find Flights']")));
        findFlightsButton.Click();

        // Wait for results table to appear
        wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath("//table//tbody//tr")));

        // Act - Find flights cheaper than maxPrice
        var flightRows = driver.FindElements(By.XPath("//table//tbody//tr"));
        var cheapFlightRow = flightRows.FirstOrDefault(row =>
        {
            var priceCell = row.FindElement(By.XPath(".//td[6]"));
            var priceText = priceCell.Text.Replace("$", "").Trim();
            if (double.TryParse(priceText, out var price))
            {
                return price < maxPrice;
            }
            return false;
        });

        // Assert
        cheapFlightRow.Should().NotBeNull(because: $"there should be at least one flight cheaper than ${maxPrice}");

        // Take screenshot of the cheap flight row and save to Desktop
        var screenshotElement = cheapFlightRow.FindElement(By.XPath(".//ancestor::tr"));
        Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        screenshot.SaveAsFile(screenshotPath);

        File.Exists(screenshotPath).Should().BeTrue(because: $"screenshot should be saved to {screenshotPath}");
    }

}

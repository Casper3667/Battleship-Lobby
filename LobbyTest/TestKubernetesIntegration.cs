using GameLobby.Kube;
using k8s;
using k8s.Models;
using Moq;

namespace LobbyTest
{
    /// <summary>
    /// Tests several methods within the FindServers class, which is used for kubernetes connection.
    /// Given the tests are done without actual kubernetes or docker connection, the real methods
    /// are expected to fail, so instead we fake most of it. Not very efficient.
    /// </summary>
    [TestFixture]
    public class FindServersTests
    {
        private Mock<FindServers> _mockFindServers;
        private Mock<Kubernetes> _kubernetesClientMock;

        /// <summary>
        /// Set up the basic components for testing.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _kubernetesClientMock = new Mock<Kubernetes>(KubernetesClientConfiguration.BuildDefaultConfig());
            _mockFindServers = new Mock<FindServers>() { CallBase = true };
            _mockFindServers.Object._client = _kubernetesClientMock.Object;
        }

        /// <summary>
        /// Check it gives a result back.
        /// </summary>
        [Test]
        public void Test_LocateIP_Success()
        {
            // Arrange
            _mockFindServers.Setup(x => x.SortServers()).ReturnsAsync("fake-ip");

            // Act
            string result = _mockFindServers.Object.locateIP();

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Check that servers are returned and that they are sorted.
        /// </summary>
        [Test]
        public async Task Test_SortServers_Success()
        {
            // Arrange
            int callCount = 0;
            _mockFindServers.Setup(x => x.GetConnectedClientsCount(It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                // Provide different results based on the call count
                if (callCount == 0)
                {
                    callCount++;
                    return 2;
                }
                else
                {
                    return 1;
                }
            });
            _mockFindServers.Setup(x => x.ListPodsAsync()).ReturnsAsync(new V1PodList
            {
                Items = new List<V1Pod>
                {
                            new() { Status = new V1PodStatus { PodIP = "1.2.3.4" } },
                            new() { Status = new V1PodStatus { PodIP = "5.6.7.8" } }
                }
            });

            // Act
            string result = await _mockFindServers.Object.SortServers();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo("5.6.7.8")); // Second result has 1 player, so that should be returned.
        }

        /// <summary>
        /// Tests that it fails, given it is not a real server.
        /// </summary>
        [Test]
        public async Task Test_GetConnectedClientsCount_Success()
        {
            // Arrange
            _mockFindServers.Setup(x => x.GetConnectedClientsCount(It.IsAny<string>())).CallBase();

            // Act
            int result = await _mockFindServers.Object.GetConnectedClientsCount("fake-pod-ip");

            // Assert
            Assert.That(result, Is.EqualTo(-1)); // Assuming the fake request will fail
        }
    }
}

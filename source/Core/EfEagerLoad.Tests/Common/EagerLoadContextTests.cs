using System;
using EfEagerLoad.Common;
using EfEagerLoad.Engine;
using EfEagerLoad.Tests.Testing.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using Xunit;

namespace EfEagerLoad.Tests.Common
{
    public class EagerLoadContextTests
    {
        [Fact]
        public void ShouldDisplayEmpty_ParentIncludePath_WhenNoNavigationsSet()
        {
            var context = new EagerLoadContext(new Mock<DbContext>().Object, new Mock<IIncludeStrategy>().Object);

            Assert.Equal(string.Empty, context.CurrentIncludePath.ToString());
        }

        [Fact]
        public void ShouldDisplayEmpty_CurrentIncludePath_WhenNoNavigationsSet()
        {
            var context = new EagerLoadContext(new Mock<DbContext>().Object, new Mock<IIncludeStrategy>().Object);

            Assert.Equal(string.Empty, context.CurrentIncludePath.ToString());
        }
        
        [Fact]
        public void ShouldDisplayCorrect_CurrentIncludePath_WhenAddingFirstNavigation()
        {
            var context = new EagerLoadContext(new Mock<DbContext>().Object, new Mock<IIncludeStrategy>().Object);
            
            var navigationMock = new Mock<INavigation>();
            navigationMock.Setup(nav => nav.Name).Returns(nameof(Book));

            context.SetCurrentNavigation(navigationMock.Object);

            Assert.Equal(nameof(Book), context.CurrentIncludePath.ToString());
        }

        [Fact]
        public void ShouldDisplayCorrect_CurrentIncludePath_WhenMoreNavigationsAdded()
        {
            var context = new EagerLoadContext(new Mock<DbContext>().Object, new Mock<IIncludeStrategy>().Object);

            var bookNavigationMock = new Mock<INavigation>();
            bookNavigationMock.Setup(nav => nav.Name).Returns(nameof(Book));
            var authorNavigationMock = new Mock<INavigation>();
            authorNavigationMock.Setup(nav => nav.Name).Returns(nameof(Author));
            var publisherNavigationMock = new Mock<INavigation>();
            publisherNavigationMock.Setup(nav => nav.Name).Returns(nameof(Publisher));

            context.SetCurrentNavigation(bookNavigationMock.Object);
            context.SetCurrentNavigation(authorNavigationMock.Object);

            Assert.Equal($"{nameof(Book)}.{nameof(Author)}", context.CurrentIncludePath.ToString());

            context.SetCurrentNavigation(publisherNavigationMock.Object);

            Assert.Equal($"{nameof(Book)}.{nameof(Author)}.{nameof(Publisher)}", context.CurrentIncludePath.ToString());
        }

        [Fact]
        public void ShouldDisplayCorrect_CurrentIncludePath_WhenRemovingNavigation()
        {
            var context = new EagerLoadContext(new Mock<DbContext>().Object, new Mock<IIncludeStrategy>().Object);

            var bookNavigationMock = new Mock<INavigation>();
            bookNavigationMock.Setup(nav => nav.Name).Returns(nameof(Book));
            var authorNavigationMock = new Mock<INavigation>();
            authorNavigationMock.Setup(nav => nav.Name).Returns(nameof(Author));
            var publisherNavigationMock = new Mock<INavigation>();
            publisherNavigationMock.Setup(nav => nav.Name).Returns(nameof(Publisher));

            context.SetCurrentNavigation(bookNavigationMock.Object);
            context.SetCurrentNavigation(authorNavigationMock.Object);
            context.SetCurrentNavigation(publisherNavigationMock.Object);

            Assert.Equal($"{nameof(Book)}.{nameof(Author)}.{nameof(Publisher)}", context.CurrentIncludePath.ToString());

            context.RemoveCurrentNavigation();
            Assert.Equal($"{nameof(Book)}.{nameof(Author)}", context.CurrentIncludePath.ToString());

            context.RemoveCurrentNavigation();
            Assert.Equal($"{nameof(Book)}", context.CurrentIncludePath.ToString());

            context.RemoveCurrentNavigation();
            Assert.Equal(string.Empty, context.CurrentIncludePath.ToString());

            context.RemoveCurrentNavigation();
            Assert.Equal(string.Empty, context.CurrentIncludePath.ToString());
        }


    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Snowbow.Test {
	public class MyUnitTest {
        private readonly ITestOutputHelper _testOutputHelper;

        public MyUnitTest(ITestOutputHelper testOutputHelper) {
            _testOutputHelper = testOutputHelper;
        }
        [Fact]
        public async Task TestPandocRenderAsync() {
            string markdown = @"# 你好，世界！
$$x=frac{-b\pm\sqrt{b^2-4ac}}{2a}$$";
            string expected = @"<h1 id=""你好世界"">你好，世界！</h1><p><math display=""block"" xmlns=""http://www.w3.org/1998/Math/MathML""><semantics><mrow><mi>x</mi><mo>=</mo><mi>f</mi><mi>r</mi><mi>a</mi><mi>c</mi><mrow><mo>−</mo><mi>b</mi><mo>±</mo><msqrt><mrow><msup><mi>b</mi><mn>2</mn></msup><mo>−</mo><mn>4</mn><mi>a</mi><mi>c</mi></mrow></msqrt></mrow><mrow><mn>2</mn><mi>a</mi></mrow></mrow><annotation encoding=""application/x-tex"">x=frac{-b\pm\sqrt{b^2-4ac}}{2a}</annotation></semantics></math></p>";
            string actual = await Subprocess.PandocRenderAsync(markdown, CancellationToken.None);
            _testOutputHelper.WriteLine(actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestParseFrontMatter() {
            string markdown = @"---
layout: page
title: 关于我
toc: !!bool true
---
## 学习经历

### 基础教育
";
            var expected = JObject.Parse("{\"layout\":\"page\",\"title\":\"关于我\",\"toc\":true}");
            var actual = Subprocess.ParseFrontMatter(markdown);
            _testOutputHelper.WriteLine(actual.ToString());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestExtractPartialHtml() {
            string contentHtml = @"<p>hello</p>
<!-- more -->
<p>world</p>";
            string expected = "<p>hello</p>\n";
            string actual = Subprocess.ExtractPartialHtml(contentHtml);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestJson() {
            string jsonStr = @"{""themes"":[""a"",""b""]}";
            var json = JToken.Parse(jsonStr);
            var temp = json["themes"].ToObject<string[]>();
            Assert.Equal("a", temp[0]);
		}
    }
}

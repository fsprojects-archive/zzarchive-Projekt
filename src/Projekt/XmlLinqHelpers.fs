namespace Projekt

open System.Xml.Linq

module XmlLinqHelpers =

  let msbuildNamespace = "{http://schemas.microsoft.com/developer/msbuild/2003}"

  let xn s = XName.Get s
  
  let xe n (v: obj) = new XElement(xn n, v)

  let xa n (v: obj) = new XAttribute(xn n, v)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoogleGiminiDemo;

public class GiminiContent
{
    public List<GiminiPart> parts { get; set; }
}

public class GiminiInlineData
{
    public string mime_type { get; set; }
    public string data { get; set; }
}

public class GiminiPart
{
    public string text { get; set; }
    public GiminiInlineData inline_data { get; set; }
}

public class GiminiPostData
{
    public List<GiminiContent> contents { get; set; }
    public GiminiContent system_instruction { get; set; }
    public GiminiConfig generationConfig { get; set; }
}

public class GiminiConfig
{
    public List<string> stopSequences { get; set; }
    public double? temperature { get; set; }
    public int? maxOutputTokens { get; set; }
    public double? topP { get; set; }
    public int? topK { get; set; }
    public string response_mime_type { get; set; }
    public JsonDocument response_schema { get; set; }
}
public class GiminiResponse
{
    public List<Candidate> candidates { get; set; }
    public UsageMetadata usageMetadata { get; set; }
    public string modelVersion { get; set; }
}

public class Candidate
{
    public Content content { get; set; }
    public string finishReason { get; set; }
    public double avgLogprobs { get; set; }
}

public class CandidatesTokensDetail
{
    public string modality { get; set; }
    public int tokenCount { get; set; }
}

public class Content
{
    public List<Part> parts { get; set; }
    public string role { get; set; }
}

public class Part
{
    public string text { get; set; }
}

public class PromptTokensDetail
{
    public string modality { get; set; }
    public int tokenCount { get; set; }
}

public class UsageMetadata
{
    public int promptTokenCount { get; set; }
    public int candidatesTokenCount { get; set; }
    public int totalTokenCount { get; set; }
    public List<PromptTokensDetail> promptTokensDetails { get; set; }
    public List<CandidatesTokensDetail> candidatesTokensDetails { get; set; }
}

public class ParseResult
{
    public ImageType image_type { get; set; }
    public string text { get; set; }
    public string title { get; set; }
    public string image_content { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImageType
{
    photo, graph, text, table, unrecognized
}
﻿namespace BagiraWebApi.Services.Parser.DTO
{
    public class ParserGoodRequestParam
    {
        public bool? HasLinkToBagira { get; set; }
        public int Take { get; set; } = 20;
        public int Skip { get; set; } = 0;
    }
}

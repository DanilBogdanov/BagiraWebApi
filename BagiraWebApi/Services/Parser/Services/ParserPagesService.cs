using BagiraWebApi.Models.Parser;
using Microsoft.EntityFrameworkCore;
using System;

namespace BagiraWebApi.Services.Parser.Services
{
    public class ParserPagesService
    {
        private readonly ApplicationContext _context;

        public ParserPagesService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<ParserPage>> GetPagesAsync(int parserCompanyId)
        {
            return await _context.ParserPages
                .AsNoTracking()
                .Where(x => x.ParserCompanyId == parserCompanyId)
                .ToListAsync();
        }

        public ParserPage AddParserPage(ParserPage parserPage)
        {
            var entity = _context.Add(parserPage).Entity;
            _context.SaveChanges();

            return entity;
        }

        public ParserPage? UpdatePage(ParserPage parserPage)
        {
            var pageExist = _context.ParserPages.AsNoTracking().Any(page => page.Id == parserPage.Id);
            if (!pageExist) return null;

            var entity = _context.Update(parserPage).Entity;
            _context.SaveChanges();

            return entity;
        }

        public bool? UpdatePageIsActive(int pageId, bool isActive)
        {
            var page = _context.ParserPages.Find(pageId);
            if (page == null) return null;

            page.IsActive = isActive;
            _context.SaveChanges();

            return page.IsActive;
        }

        public ParserPage? DeletePage(int parserPageId)
        {
            var page = _context.ParserPages.Find(parserPageId);
            if (page != null)
            {
                var entity = _context.Remove(page).Entity;
                _context.SaveChanges();
                return entity;
            }

            return null;
        }
    }
}

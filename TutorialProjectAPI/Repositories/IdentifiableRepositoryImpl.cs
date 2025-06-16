using Microsoft.EntityFrameworkCore;
using TutorialProjectAPI.Contexts;

namespace TutorialProjectAPI.Repositories
{
    // ⬇️  CHANGE HERE: inherit from IIdentifiableRepository<T>
    public class IdentifiableRepositoryImpl<T> : IIdentifiableRepository<T> where T : class
    {
        private readonly MainContext _context;
        private readonly DbSet<T> _dbSet;

        public IdentifiableRepositoryImpl(MainContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(Guid id, bool track = false)
        {
            var query = _context.Set<T>().Where(e => EF.Property<Guid>(e, "Id") == id);
            if (!track) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }
        public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}

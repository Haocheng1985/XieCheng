using Microsoft.EntityFrameworkCore;
using MyFakexiecheng.Database;
using MyFakexiecheng.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Services
{
    public class TouristRouteRepository : ITouristRouteRepository
    {
        private readonly AppDbContext _context;

        public TouristRouteRepository(AppDbContext context)
        {
            _context = context;
        }

        

        public TouristRoute GetTouristRoute(Guid touristRouteId)
        {
            return _context.TouristRoutes.Include(t => t.TouristRoutePictures).FirstOrDefault(n => n.Id == touristRouteId);
        }

        public IEnumerable<TouristRoute> GetTouristRoutes(
            string keyword,
            string ratingOperator, 
            int? ratingValue)
        { 
            //include vs join(manual) eager load. another is lazyload
            IQueryable<TouristRoute> result = _context.TouristRoutes.Include(t => t.TouristRoutePictures);//generate sql, not for database exec
            if (!string.IsNullOrWhiteSpace(keyword)) {
                keyword = keyword.Trim();
                result = result.Where(t => t.Title.Contains(keyword));//where function to generate sql 
            }
            if (ratingValue >= 0)
            {
                result = ratingOperator switch
                {
                    "largerThan" => result.Where(t => t.Rating >= ratingValue),
                    "lessThan" => result.Where(t => t.Rating <= ratingValue),
                    _ => result.Where(t => t.Rating == ratingValue),
                };
            }

            return result.ToList();//tolist is iqueryable's function, exec database access right way, then get data from database.
        }

        public bool TouristRouteExists(Guid touristRouteId)
        {
            return _context.TouristRoutes.Any(t => t.Id == touristRouteId);
        }
        
        public IEnumerable<TouristRoutePicture> GetPicturesByTouristRouteId(Guid touristRouteId)
        {
            return _context.TouristRoutePictures.Where(p => p.TouristRouteId == touristRouteId).ToList();
        }

        public TouristRoutePicture GetPicture(int pictureId)
        {
            return _context.TouristRoutePictures.Where(p => p.Id == pictureId).FirstOrDefault();
        }

        public void AddTouristRoute(TouristRoute touristRoute)
        {
            if (touristRoute == null)
            {
                throw new ArgumentNullException(nameof(touristRoute));
            }
            _context.TouristRoutes.Add(touristRoute);
            //_context.SaveChanges();
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);//save data to database
        }

        public void AddTouristRoutePicture(Guid touristRouteId, TouristRoutePicture touristRoutePicture)
        {
            if (touristRouteId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(touristRouteId));
            }
            if (touristRoutePicture == null)
            {
                throw new ArgumentNullException(nameof(touristRoutePicture));
            }
            touristRoutePicture.TouristRouteId = touristRouteId;
            _context.TouristRoutePictures.Add(touristRoutePicture);
        }

        public void DeleteTouristRoute(TouristRoute touristRoute)
        {
            _context.TouristRoutes.Remove(touristRoute);
        }

        public void DeleteTouristRoutePicture(TouristRoutePicture picture)
        {
            
                _context.TouristRoutePictures.Remove(picture);
            
        }

        public void DeleteTouristRoutes(IEnumerable<TouristRoute> touristRoutes)
        {
            _context.TouristRoutes.RemoveRange(touristRoutes);
        }

        public IEnumerable<TouristRoute> GetTouristRoutesByIDList(IEnumerable<Guid> ids)
        {
            return _context.TouristRoutes.Where(t => ids.Contains(t.Id)).ToList();
        }
    }
}

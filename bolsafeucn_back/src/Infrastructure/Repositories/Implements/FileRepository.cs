using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Data;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Implements
{
    public class FileRepository : IFileRepository
    {
        private readonly AppDbContext _context;

        public FileRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea un archivo de imagen en la base de datos.
        /// </summary>
        /// <param name="file">El archivo de imagen a crear.</param>
        /// <returns>True si el archivo se creó correctamente, de lo contrario false y null en caso de que la imagen ya existe.</returns>
        public async Task<bool?> CreateAsync(Image file)
        {
            var existsImage = await _context.Images.AnyAsync(i => i.PublicId == file.PublicId);
            if (!existsImage)
            {
                _context.Images.Add(file);
                return await _context.SaveChangesAsync() > 0;
            }
            return null;
        }

        /// <summary>
        /// Crea un archivo de imagen de usuario en la base de datos.
        /// </summary>
        /// <param name="file">El archivo de imagen a crear.</param>
        /// <returns>True si el archivo se creó correctamente, de lo contrario false y null en caso de que la imagen ya existe.</returns>
        public async Task<bool?> CreateUserImageAsync(UserImage file)
        {
            var existsImage = await _context.UserImages.AnyAsync(i => i.PublicId == file.PublicId);
            if (!existsImage)
            {
                _context.UserImages.Add(file);
                return await _context.SaveChangesAsync() > 0;
            }
            return null;
        }

        /// <summary>
        /// Elimina un archivo de imagen de la base de datos.
        /// </summary>
        /// <param name="publicId">El identificador público del archivo a eliminar.</param>
        /// <returns>True si el archivo se eliminó correctamente, de lo contrario false y null si la imagen no existe.</returns>
        public async Task<bool?> DeleteAsync(string publicId)
        {
            var image = await _context.Images.FirstOrDefaultAsync(i => i.PublicId == publicId);
            if (image != null)
            {
                _context.Images.Remove(image);
                return await _context.SaveChangesAsync() > 0;
            }
            return null;
        } 
    }
}
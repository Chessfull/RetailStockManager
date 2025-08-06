using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailStockManager.Application.DTOs.Common
{
    public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages)
    {
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int ItemsOnCurrentPage => Items.Count();

        public static PagedResultDto<T> Create(
            IEnumerable<T> items,
            int currentPage,
            int pageSize,
            int totalItems)
        {
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PagedResultDto<T>(
                items,
                currentPage,
                pageSize,
                totalItems,
                totalPages);
        }
    }
}

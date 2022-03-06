using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Blog
{
    [Table("Category")]
    public class Category
    {

        [Key]
        public int Id { get; set; }

        // Tiều đề Category
        [Required(ErrorMessage = "Please enter title")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} has at least {1} to {2} characters")]
        [Display(Name = "Title")]
        public string? Title { get; set; }

        // Nội dung, thông tin chi tiết về Category
        [DataType(DataType.Text)]
        [Display(Name = "Description")]
        public string? Description { set; get; }

        //chuỗi Url
        [Required(ErrorMessage = "Please enter url")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} has at least {1} to {2} characters")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Using [a-z0-9] only")]
        [Display(Name = "Url")]
        public string? Slug { set; get; }

        // Các Category con
        public ICollection<Category>? CategoryChildren { get; set; }

        // Category cha (FKey)
        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        [Display(Name = "Parent Category")]

        public Category? ParentCategory { set; get; }


        public void ChildCategoryIDs(List<int> lists, ICollection<Category>? childcates = null)
        {
            if (childcates == null)
                childcates = this.CategoryChildren;

            foreach (Category category in childcates)
            {
                lists.Add(category.Id);
                ChildCategoryIDs(lists, category.CategoryChildren);
            }
        }

        public List<Category> ListParents()
        {
            List<Category> li = new List<Category>();
            var parent = this.ParentCategory;
            while (parent != null)
            {
                li.Add(parent);
                parent = parent.ParentCategory;
            }
            li.Reverse();
            return li;
        }
    }
}
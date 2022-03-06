using App.Data;
using App.Database;
using App.Models;
using App.Utilities;
using App.Areas.Blog.Models;
using blogProject.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models.Blog;

namespace blogProject.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly MyBlogContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PostController(MyBlogContext context, UserManager<AppUser> userManager, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        [TempData]
        public string StatusMessage { get; set; }
        // GET: Blog/Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            var user = await _userManager.GetUserAsync(this.User);

            var rs = await _userManager.IsInRoleAsync(user, "Administrator");

            IOrderedQueryable<App.Models.Blog.Post>? posts = null;
            if (rs)
            {
                posts = _context.Posts
                        .Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);
            }
            else
            {
                posts = _context.Posts
                        .Include(p => p.Author)
                        .Where(p => p.Author.Id == user.Id)
                        .OrderByDescending(p => p.DateUpdated);
            }

            int totalPosts = await posts.CountAsync();
            if (pagesize <= 0) pagesize = 10;
            int countPages = (int)Math.Ceiling((double)totalPosts / pagesize);

            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pagesize
                })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            ViewBag.postIndex = (currentPage - 1) * pagesize;

            var postsInPage = await posts.Skip((currentPage - 1) * pagesize)
                             .Take(pagesize)
                             .Include(p => p.PostCategories)
                             .ThenInclude(pc => pc.Category)
                             .ToListAsync();

            return View(postsInPage);
        }

        // GET: Blog/Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var user = await _userManager.GetUserAsync(this.User);

            var rs = await _userManager.IsInRoleAsync(user, "Administrator");

            App.Models.Blog.Post? post = null;

            if (id == null)
            {
                return NotFound();
            }

            if (rs)
            {

                post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            }
            else
            {
                post = await _context.Posts
                .Include(p => p.Author)
                .Where(p => p.Author.Id == user.Id)
                .FirstOrDefaultAsync(m => m.PostId == id);
            }

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Blog/Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.Categories.ToListAsync();

            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            return View();
        }

        // POST: Blog/Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post, IFormFile ImageFile)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Please enter another url string");
                return View(post);
            }

            if (ModelState.IsValid)
            {

                var user = await _userManager.GetUserAsync(this.User);
                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;
                _context.Add(post);

                if (post.CategoryIDs != null)
                {
                    foreach (var CateId in post.CategoryIDs)
                    {
                        _context.Add(new PostCategory()
                        {
                            CategoryID = CateId,
                            Post = post
                        });
                    }
                }

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var title = post.Title;
                    string _FileName = "";
                    int index = ImageFile.FileName.IndexOf('.');
                    _FileName = "Post" + "-" + DateTime.Now.ToString("yymmssfff") + title + "." + ImageFile.FileName.Substring(index + 1);
                    string _path = Path.Combine("Uploads/post/", _FileName);
                    await ImageFile.CopyToAsync(new FileStream(_path, FileMode.Create));
                    post.ImageFile = _FileName;
                }
                _context.Add(post);


                await _context.SaveChangesAsync();
                StatusMessage = "New post created";
                return RedirectToAction(nameof(Index));
            }


            return View(post);
        }

        // GET: Blog/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var user = await _userManager.GetUserAsync(this.User);

            var rs = await _userManager.IsInRoleAsync(user, "Administrator");

            App.Models.Blog.Post? post = null;

            if (id == null)
            {
                return NotFound();
            }

            if (rs)
            {
                // var post = await _context.Posts.FindAsync(id);
                post = await _context.Posts
                    .Include(p => p.PostCategories)
                    .FirstOrDefaultAsync(p => p.PostId == id);
            }
            else
            {
                post = await _context.Posts
                    .Include(p => p.PostCategories)
                    .Where(p => p.Author.Id == user.Id)
                    .FirstOrDefaultAsync(p => p.PostId == id);
            }

            if (post == null)
            {
                return NotFound();
            }

            var postEdit = new CreatePostModel()
            {
                PostId = post.PostId,
                ImageFile = post.ImageFile,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                CategoryIDs = post.PostCategories.Select(pc => pc.CategoryID).ToArray()
            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            return View(postEdit);
        }

        // POST: Blog/Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,ImageFile,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post, IFormFile? ImageFile)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");


            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                ModelState.AddModelError("Slug", "Enter another url");
                return View(post);
            }

            if (post.ImageFile == null)
                post.ImageFile = _context.Posts.Where(p => p.PostId == id).Select(p => p.ImageFile).FirstOrDefault();

            if (ModelState.IsValid)
            {
                try
                {

                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate == null)
                    {
                        return NotFound();
                    }

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var title = post.Title;
                        string _FileName = "";
                        int index = ImageFile.FileName.IndexOf('.');
                        _FileName = "Post" + "-" + DateTime.Now.ToString("yymmssfff") + title + "." + ImageFile.FileName.Substring(index + 1);
                        string _path = Path.Combine("Uploads/post/", _FileName);
                        using (var stream = new FileStream(_path, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }
                        // await ImageFile.CopyToAsync(new FileStream(_path, FileMode.Create));
                        post.ImageFile = _FileName;
                    }

                    postUpdate.Title = post.Title;
                    postUpdate.ImageFile = post.ImageFile;
                    postUpdate.Description = post.Description;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateUpdated = DateTime.Now;

                    // Update PostCategory
                    if (post.CategoryIDs == null) post.CategoryIDs = new int[] { };

                    var oldCateIds = postUpdate.PostCategories.Select(c => c.CategoryID).ToArray();
                    var newCateIds = post.CategoryIDs;

                    var removeCatePosts = from postCate in postUpdate.PostCategories
                                          where (!newCateIds.Contains(postCate.CategoryID))
                                          select postCate;
                    _context.PostCategories.RemoveRange(removeCatePosts);

                    var addCateIds = from CateId in newCateIds
                                     where !oldCateIds.Contains(CateId)
                                     select CateId;

                    foreach (var CateId in addCateIds)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            PostID = id,
                            CategoryID = CateId
                        });
                    }

                    _context.Update(postUpdate);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "You just updated post";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

        // GET: Blog/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var user = await _userManager.GetUserAsync(this.User);

            var rs = await _userManager.IsInRoleAsync(user, "Administrator");

            App.Models.Blog.Post? post = null;

            if (id == null)
            {
                return NotFound();
            }

            if (rs)
            {
                post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            }
            else
            {
                post = await _context.Posts
                .Include(p => p.Author)
                .Where(p => p.Author.Id == user.Id)
                .FirstOrDefaultAsync(m => m.PostId == id);
            }

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Blog/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            StatusMessage = "You just delete post: " + post.Title;

            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }

        public void AddImage(Post _post)
        {

            // string fileName = Path.GetFileNameWithoutExtension(_post.ImageFile.FileName);
            // string extension = Path.GetExtension(_post.ImageFile.FileName);
            // fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension; //change filename with different format (EX: abc215011996.jpg)
            // _post.postImg = "~/Uploads/" + fileName; // assign supImg image to pointed folder path
            // fileName = Path.Combine(_hostingEnvironment.WebRootPath("~/Uploads/"), fileName); //combine folder path
            // _post.ImageFile.CopyTo; // save file as above path


        }

    }
}
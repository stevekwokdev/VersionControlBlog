﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlogApplication.Models;
using BlogApplication.Data;
using BlogApplication.Data.Repository;
using Microsoft.AspNetCore.Authorization;
using BlogApplication.Models.Comment;

namespace BlogApplication.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private IRepository _repository;

        /// <summary>
        /// This is the constructor of repository 
        /// </summary>
        public HomeController(IRepository repository)
        {
            _repository = repository;
            var comment = new MainComment();
        }

        /// <summary>
        /// This action method uses the get all post from repository , and display all the post in view Post
        /// </summary>
        /// <returns>A view with List of posts</returns>    
        [AllowAnonymous]
        public IActionResult Post()
        {
            var posts = _repository.GetAllPosts();
            return View(posts);
        }

        /// <summary>
        /// This action method provide a view PanelTest to List all the post and giving a option to edit and remove
        /// Admin only 
        /// </summary>
        /// <returns>Post action method</returns>
        [Authorize("Panel")]
        public IActionResult Panel()
        {           
            return Post();
        }

        /// <summary>
        /// This action method check if the postid is valid and gives a view EditPost
        /// Admin only
        /// </summary>
        /// <param name="id">Post Id</param>
        /// <returns>if the Post id is nothing then redirect the page to the Post page, otherwise pass the id</returns>
        [HttpGet]
        [Authorize("Edit Post")]
        public IActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Post");
            }
            else
            {
                var post = _repository.GetPost(id);
                return View(post);
            }
        }

        /// <summary>
        /// This action method check if the postid match and update the post and save changes
        /// Admin only
        /// </summary>
        /// <param name="id">PostId</param>
        /// <param name="post">The post</param>
        /// <returns>if the the post id doesnt match redirect to post page , otherwise update post save changes and redirect to post page </returns>
        [HttpPost]
        //[Authorize(Policy = "Admin")]
        [Authorize("Edit Post")]
        public async Task<IActionResult> EditPost(int id, Post post)
        {
            if (id != post.PostId)
            {
                return RedirectToAction("Post");
            }
            else
            {   
                _repository.UpdatePost(post);
                await _repository.SaveChangesAsync();
                return RedirectToAction("Post");                       
            }
        }

        /// <summary>
        /// This action method gives a view CreatePost
        /// Admin Only
        /// </summary>
        /// <returns>new post</returns>
        [HttpGet]
        [Authorize("Create Post")]
        public IActionResult CreatePost()
        {
           return View(new Post());
        }

        /// <summary>
        /// This action method to let user comment in blog
        /// </summary>
        /// <param name="cvm">commentViewModel</param>
        /// <returns>A specific blog page</returns>
        [HttpPost]
        [Authorize("Comment")]
        public async Task<IActionResult> Comment(CommentViewModel cvm)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Post", new { id = cvm.PostId });

            var post = _repository.GetPost(cvm.PostId);

            if(cvm.MainCommentId == 0)
            {
                post.MainComments = post.MainComments ?? new List<MainComment>();

                post.MainComments.Add(new MainComment
                {
                    Message = cvm.Message,
                    CreatedTime = DateTime.Now,
                    Author = User.Identity.Name,

                });

                _repository.UpdatePost(post);
            }
            else
            {
                var comment = new SubComment
                {
                    MainCommentId = cvm.MainCommentId,
                    Message = cvm.Message,
                    CreatedTime = DateTime.Now,
                    Author = User.Identity.Name,
                };
                _repository.AddSubComment(comment);

            }

            await _repository.SaveChangesAsync();


            return RedirectToAction("Details", new { id = cvm.PostId });
        }

        /// <summary>
        /// This action method create a post also store the author which is username
        /// Admin only
        /// </summary>
        /// <param name="post">The post</param>
        /// <returns>Redirect to Post page</returns>
        [HttpPost]
        //[Authorize(Policy = "Admin")]
        [Authorize("Create Post")]
        public async Task<IActionResult> CreatePost(Post post)
        {
            post.Author = User.Identity.Name;
            _repository.CreatePost(post);
            await _repository.SaveChangesAsync();
            return RedirectToAction("Post");
        }

        /// <summary>
        /// This action method remove the post
        /// Admin only
        /// </summary>
        /// <param name="id">Post Id</param>
        /// <param name="post">The post</param>
        /// <returns></returns>
        //[Authorize(Policy = "Admin")]
        [Authorize("Remove Post")]
        public async Task<IActionResult> RemovePost(int? id , Post post)
        {
            post = _repository.GetPost(id);
            _repository.RemovePost(post);
            await _repository.SaveChangesAsync();
            return RedirectToAction("Panel");
        }

        /// <summary>
        /// Error Message generated by m$
        /// </summary>
        /// <returns>Error page</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// A Specific Blog page
        /// </summary>
        /// <param name="id">blog id</param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult Details(int? id)
        {
            return View(_repository.GetPost(id));
        }
    }
}

﻿using AutoMapper;
using DAL;
using DAL.Models;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RedTenAngular.Controllers
{
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public GamesController(IMapper mapper, IUnitOfWork unitOfWork, ILogger<GamesController> logger)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Game> GetGames()
        {
            var games = _unitOfWork.Games.GetAllGames(_unitOfWork.CurrentUserId);
            return games;
        }

        [HttpGet("{id}")]
        public GameDetails GetGame(int id)
        {
           
            GameDetails game = this._unitOfWork.Games.GetGame(id, this._unitOfWork.CurrentUserId);
            return game;
        }

        [HttpPost]
        public IActionResult PostGames(Game game)
        {
            int? groupId = this._unitOfWork.GroupUsers.GetGroupId(this._unitOfWork.CurrentUserId);
            if (!groupId.HasValue)
            {
                return BadRequest("Please create your group first");
            }
            game.GroupId = groupId.Value;
            this._unitOfWork.Games.AddGame(game);
            this._unitOfWork.SaveChanges();
            return Ok(game);
        }

        [HttpPut]
        public IActionResult PutGame([Required][FromBody]Game game)
        {
            int? groupId = _unitOfWork.GroupUsers.GetGroupId(_unitOfWork.CurrentUserId);
            if (!groupId.HasValue)
            {
                return BadRequest("Please create your group first");
            }

            int idFromBody = game.id;
            Game gameFromDB = _unitOfWork.Games.GetGame(idFromBody, _unitOfWork.CurrentUserId);
            int idFromGame = gameFromDB.id;

            // Security validation
            if (game.GroupId == groupId && idFromBody == idFromGame)
            {
                gameFromDB.Status = game.Status;
                gameFromDB.Location = game.Location;
                gameFromDB.Date = game.Date;
                _unitOfWork.Games.UpdateGame(gameFromDB);
                _unitOfWork.SaveChanges();
                return Ok(game);
            }

            return BadRequest("Validation Error");
            
        }
    }
}

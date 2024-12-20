﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Services
{
    public class ProjectService
    {
        private readonly RabbitMqService _rabbitMqService;

        public ProjectService(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        public async Task<ProjectModel> GetProject(int id)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();
                var message = JsonConvert.SerializeObject(new
                {
                    Action = "Get",
                    ProjectId = id,
                    CorrelationId = correlationId
                });

                ProjectModel model = new ProjectModel();

                _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);
                var responseMessage = await _rabbitMqService.GetAndRemoveMessage(correlationId);

                if (!string.IsNullOrEmpty(responseMessage))
                {
                    var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                    if (response.Action == "Get" && response.CorrelationId == correlationId)
                    {
                        if (response.Status == "Success")
                        {
                            model.Id = response.Project.Id;
                            model.Title = response.Project.Title;
                            model.FreelancerId = response.Project.FreelancerId;
                            model.Budget = response.Project.Budget;
                            model.ClientId = response.Project.ClientId;
                            model.Description = response.Project.Description;
                            model.CreatedAt = response.Project.CreatedAt;
                            model.UpdatedAt = response.Project.UpdatedAt;
                        }
                        return model;
                    }
                }
                return new ProjectModel();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new ProjectModel();
            }
        }

        public async Task Create(string id, ProjectModel project)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(new
            {
                Action = "Create",
                Title = project.Title,
                Budget = project.Budget,
                Description = project.Description,
                ClientId = id,
                FreelancerId = (int?)null,
                CorrelationId = correlationId
            });
            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            var responseMessage = await _rabbitMqService.GetAndRemoveMessage(correlationId);
        }

        public async Task<List<ProjectModel>> Find(string[] tags)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();
                var message = JsonConvert.SerializeObject(new
                {
                    Action = "Find",
                    Tags = tags,
                    CorrelationId = correlationId
                });
                _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);
                var responseMessage = await _rabbitMqService.GetAndRemoveMessage(correlationId);
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);

                if (response.Status == "Success")
                {
                    var projectList = JsonConvert.DeserializeObject<List<ProjectModel>>(response.Projects.ToString());
                    return projectList;
                }
                return new List<ProjectModel>();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new List<ProjectModel>();
            }
        }

        public async Task Update(int id, ProjectModel project)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();
                var message = JsonConvert.SerializeObject(new
                {
                    Action = "Update",
                    ProjectId = id,
                    Title = project.Title,
                    Budget = project.Budget,
                    Description = project.Description,
                    CorrelationId = correlationId,
                    Status = project.Status
                });

                _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

                var responseMessage = await _rabbitMqService.GetAndRemoveMessage(correlationId);
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<dynamic> Delete(int id)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();
                var message = JsonConvert.SerializeObject(new
                {
                    Action = "Delete",
                    ProjectId = id,
                    CorrelationId = correlationId
                });

                _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

                var responseMessage = await _rabbitMqService.GetAndRemoveMessage(correlationId);
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);

                return response;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        public async Task<List<ProjectModel>> GetProjectsForFreelancerAsync(int freelancerId)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();
                var message = JsonConvert.SerializeObject(new
                {
                    Action = "GetAllFreelancer",
                    FreelancerId = freelancerId,
                    CorrelationId = correlationId
                });

                _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);
                var responseMessage = await _rabbitMqService.GetAndRemoveMessage(correlationId);

                return ParseProjectResponse(responseMessage, correlationId, "GetAllFreelancer");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new List<ProjectModel>();
            }
        }

        public async Task<List<ProjectModel>> GetProjectsForClientAsync(int clientId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(new
            {
                Action = "GetAllClient",
                ClientId = clientId,
                CorrelationId = correlationId
            });

            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);
            var responseMessage = await _rabbitMqService.GetAndRemoveMessage(correlationId);

            return ParseProjectResponse(responseMessage, correlationId, "GetAllClient");
        }

        public async Task<List<ProjectModel>> GetList()
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();
                var message = JsonConvert.SerializeObject(new
                {
                    Action = "GetList",
                    CorrelationId = correlationId
                });

                List<ProjectModel> model = new List<ProjectModel>();

                _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

                var responseMessage = await _rabbitMqService.GetAndRemoveMessage(correlationId);
                return ParseProjectResponse(responseMessage, correlationId, "GetList");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new List<ProjectModel>();
            }
        }

        private List<ProjectModel> ParseProjectResponse(string responseMessage, string correlationId, string expectedAction)
        {
            var model = new List<ProjectModel>();
            try
            {
                if (!string.IsNullOrEmpty(responseMessage))
                {
                    var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                    if (response.Action == expectedAction && response.CorrelationId == correlationId)
                    {
                        if (response.Status == "Success")
                        {
                            foreach (var project in response.Projects)
                            {
                                model.Add(new ProjectModel
                                {
                                    Id = project.Id,
                                    Title = project.Title,
                                    Description = project.Description,
                                    Budget = project.Budget,
                                    ClientId = project.ClientId,
                                    FreelancerId = project.FreelancerId,
                                    CreatedAt = project.CreatedAt,
                                    UpdatedAt = project.UpdatedAt
                                });
                            }
                        }
                    }
                }

            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
            return model;
        }
    }
}

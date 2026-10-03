using Newtonsoft.Json;
using SiteSyncerAria_DrDoosti.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace SiteSyncerAria_DrDoosti
{
    public class ProductStockService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl; // آدرس اصلی API              

        public ProductStockService(string baseUrl,HttpClient httpClient)
        {
            //var handler = new HttpClientHandler()
            //{
            //    Proxy = new WebProxy("http://127.0.0.1:8888", false),
            //    UseProxy = true
            //};
            //_httpClient = new HttpClient(handler);
            _httpClient = httpClient;
            _baseUrl = baseUrl.TrimEnd('/') + "/";
        }

        public async Task<responseBody> ProductS(ListProduct products,string apiKey)
        {

            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback +=
            //(sender, cert, chain, sslPolicyErrors) => true;

            //_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            string apiUrl = $"{_baseUrl}products";

            //_httpClient.DefaultRequestHeaders.Accept.Clear();
            //_httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string jsonPayload = JsonConvert.SerializeObject(products);
            //var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response;

            try
            {
                response = await PostJsonAsync(apiUrl, jsonPayload, apiKey);
            }
            catch (TaskCanceledException ex)
            {
                return new responseBody
                {
                    message = $"خطای تایم‌اوت ارتباط با سرور (Timeout): {ex.Message}",
                    success = false,
                    dtos = null
                };
            }
            catch (HttpRequestException ex)
            {
                return new responseBody
                {
                    message = $"خطای شبکه یا اتصال HTTP: {ex.Message} | جزئیات: {ex.InnerException?.Message}",
                    success = false,
                    dtos = null
                };
            }
            catch (Exception ex)
            {
                return new responseBody
                {
                    message = $"خطای ناشناخته در ارسال درخواست: {ex.Message}",
                    success = false,
                    dtos = null
                };
            }            
            using (response)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                if ((int)response.StatusCode == 400)
                {
                    return new responseBody
                    {
                        message = $"بدنه json نیست",
                        success = false,
                        dtos = null,
                    };
                }
                else if ((int)response.StatusCode == 422)
                {
                    return new responseBody
                    {
                        message = $"بدنه خالی یا ناقص است",
                        success = false,
                        dtos = null,
                    };
                }
                else if ((int)response.StatusCode == 401)
                {
                    return new responseBody
                    {
                        message = $"کلید اشتباه",
                        success = false,
                        dtos = null,
                    };
                }
                else if (!response.IsSuccessStatusCode)
                {
                    return new responseBody
                    {
                        message = $"HTTP Error: {response.StatusCode} - {responseBody}",
                        success = false,
                        dtos = null,
                    };
                }
                else
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dto>(responseBody);
                        if (result != null)
                        {
                            return new responseBody
                            {
                                message = result.ok ? null : (result.error ?? "API returned ok=false."),
                                success = result.ok,
                                dtos = result,
                            };
                        }
                        else
                        {
                            return new responseBody
                            {
                                message = "Received an empty or invalid response from the server.",
                                success = false,
                                dtos = null,
                            };
                        }
                    }
                    catch (JsonException jsonEx)
                    {

                        return new responseBody
                        {
                            message = $"Failed to parse server response. JSON Error: {jsonEx.Message}. Raw response: {responseBody}",
                            success = false,
                            dtos = null,
                        };
                    }
                }
            }
                
        }

        public async Task<responseBody> invoicedetails(Invoice invoice,string apiKey)
        {
            string apiUrl = $"{_baseUrl}invoices";
            //_httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string jsonPayload = JsonConvert.SerializeObject(invoice);
            //var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            try
            {
                response = await PostJsonAsync(apiUrl, jsonPayload, apiKey);
            }
            catch (TaskCanceledException ex)
            {
                return new responseBody
                {
                    message = $"خطای تایم‌اوت ارتباط با سرور (Timeout): {ex.Message}",
                    success = false,
                    dtos = null
                };
            }
            catch (HttpRequestException ex)
            {
                return new responseBody
                {
                    message = $"خطای شبکه یا اتصال HTTP: {ex.Message} | جزئیات: {ex.InnerException?.Message}",
                    success = false,
                    dtos = null
                };
            }
            catch (Exception ex)
            {
                return new responseBody
                {
                    message = $"خطای ناشناخته در ارسال درخواست: {ex.Message}",
                    success = false,
                    dtos = null
                };
            }
            using (response)
            {

                string responseBody = await response.Content.ReadAsStringAsync();
                if ((int)response.StatusCode == 400)
                {
                    return new responseBody
                    {
                        message = $"بدنه json نیست",
                        success = false,
                        dtos = null,
                    };
                }
                else if ((int)response.StatusCode == 422)
                {
                    return  new responseBody
                    {
                        message = $"بدنه خالی یا ناقص است",
                        success = false,
                        dtos = null,
                    };
                }
                else if ((int)response.StatusCode == 401)
                {
                    return new responseBody
                    {
                        message = $"کلید اشتباه",
                        success = false,
                        dtos = null,
                    };
                }
                else if (!response.IsSuccessStatusCode)
                {
                    return new responseBody
                    {
                        message = $"HTTP Error: {response.StatusCode} - {responseBody}",
                        success = false,
                        dtos = null,
                    };
                }
                else
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Dto>(responseBody);
                        if (result != null)
                        {
                            return new responseBody
                            {
                                message = result.ok ? null : (result.error ?? "API returned ok=false."),
                                success = result.ok,
                                dtos = result,
                            };
                        }
                        else
                        {
                            return new responseBody
                            {
                                message = "Received an empty or invalid response from the server.",
                                success = false,
                                dtos = null,
                            };
                        }
                    }
                    catch (JsonException jsonEx)
                    {

                        return new responseBody
                        {
                            message = $"Failed to parse server response. JSON Error: {jsonEx.Message}. Raw response: {responseBody}",
                            success = false,
                            dtos = null,
                        };
                    }
                }
            }

        }

        private async Task<HttpResponseMessage> PostJsonAsync(string url, string jsonPayload, string apiKey)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Add("X-API-KEY", apiKey);
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                request.Content = new StringContent(
                    jsonPayload, Encoding.UTF8, "application/json");

                return await _httpClient.SendAsync(request);
            }
        }
    }
}


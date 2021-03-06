using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using UniswapDataApi.Models;
using UniswapDataApi.Models.DTOs;

namespace UniswapDataApi.Functions
{
    public class GetUniswapData
    {
        private readonly HttpClient _client;
        private readonly IWeb3 _web3;

        private readonly int _cacheExpirationSecs;
        private readonly string _blocklyticsApiKey;

        private const string DecimalFormatter = "0.##########";
        private const string HexUniswapAddress = "0x05cde89ccfa0ada8c88d5a23caaa79ef129e7883";

        private static DateTime _lastUpdated = DateTime.MinValue;
        private static IEnumerable<UniswapPair> _pairs = new List<UniswapPair>();

        private readonly Contract _hexContract;

        public GetUniswapData(IHttpClientFactory httpClientFactory, IWeb3 web3, IConfiguration config)
        {
            _client = httpClientFactory.CreateClient();
            _web3 = web3;
            _hexContract = web3.Eth.GetContract("[{\"inputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Approval\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data1\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"bytes20\",\"name\":\"btcAddr\",\"type\":\"bytes20\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"claimToAddr\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"referrerAddr\",\"type\":\"address\"}],\"name\":\"Claim\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data1\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data2\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"senderAddr\",\"type\":\"address\"}],\"name\":\"ClaimAssist\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"updaterAddr\",\"type\":\"address\"}],\"name\":\"DailyDataUpdate\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"uint40\",\"name\":\"stakeId\",\"type\":\"uint40\"}],\"name\":\"ShareRateChange\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data1\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"stakerAddr\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint40\",\"name\":\"stakeId\",\"type\":\"uint40\"}],\"name\":\"StakeEnd\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data1\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"stakerAddr\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint40\",\"name\":\"stakeId\",\"type\":\"uint40\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"senderAddr\",\"type\":\"address\"}],\"name\":\"StakeGoodAccounting\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"stakerAddr\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint40\",\"name\":\"stakeId\",\"type\":\"uint40\"}],\"name\":\"StakeStart\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"Transfer\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"memberAddr\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"entryId\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"referrerAddr\",\"type\":\"address\"}],\"name\":\"XfLobbyEnter\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"data0\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"memberAddr\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"entryId\",\"type\":\"uint256\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"referrerAddr\",\"type\":\"address\"}],\"name\":\"XfLobbyExit\",\"type\":\"event\"},{\"payable\":true,\"stateMutability\":\"payable\",\"type\":\"fallback\"},{\"constant\":true,\"inputs\":[],\"name\":\"allocatedSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"address\",\"name\":\"owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"}],\"name\":\"allowance\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"approve\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"rawSatoshis\",\"type\":\"uint256\"},{\"internalType\":\"bytes32[]\",\"name\":\"proof\",\"type\":\"bytes32[]\"},{\"internalType\":\"address\",\"name\":\"claimToAddr\",\"type\":\"address\"},{\"internalType\":\"bytes32\",\"name\":\"pubKeyX\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"pubKeyY\",\"type\":\"bytes32\"},{\"internalType\":\"uint8\",\"name\":\"claimFlags\",\"type\":\"uint8\"},{\"internalType\":\"uint8\",\"name\":\"v\",\"type\":\"uint8\"},{\"internalType\":\"bytes32\",\"name\":\"r\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"s\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"autoStakeDays\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"referrerAddr\",\"type\":\"address\"}],\"name\":\"btcAddressClaim\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"bytes20\",\"name\":\"\",\"type\":\"bytes20\"}],\"name\":\"btcAddressClaims\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"bytes20\",\"name\":\"btcAddr\",\"type\":\"bytes20\"},{\"internalType\":\"uint256\",\"name\":\"rawSatoshis\",\"type\":\"uint256\"},{\"internalType\":\"bytes32[]\",\"name\":\"proof\",\"type\":\"bytes32[]\"}],\"name\":\"btcAddressIsClaimable\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"bytes20\",\"name\":\"btcAddr\",\"type\":\"bytes20\"},{\"internalType\":\"uint256\",\"name\":\"rawSatoshis\",\"type\":\"uint256\"},{\"internalType\":\"bytes32[]\",\"name\":\"proof\",\"type\":\"bytes32[]\"}],\"name\":\"btcAddressIsValid\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"pure\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"address\",\"name\":\"claimToAddr\",\"type\":\"address\"},{\"internalType\":\"bytes32\",\"name\":\"claimParamHash\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"pubKeyX\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"pubKeyY\",\"type\":\"bytes32\"},{\"internalType\":\"uint8\",\"name\":\"claimFlags\",\"type\":\"uint8\"},{\"internalType\":\"uint8\",\"name\":\"v\",\"type\":\"uint8\"},{\"internalType\":\"bytes32\",\"name\":\"r\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"s\",\"type\":\"bytes32\"}],\"name\":\"claimMessageMatchesSignature\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"pure\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"currentDay\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"dailyData\",\"outputs\":[{\"internalType\":\"uint72\",\"name\":\"dayPayoutTotal\",\"type\":\"uint72\"},{\"internalType\":\"uint72\",\"name\":\"dayStakeSharesTotal\",\"type\":\"uint72\"},{\"internalType\":\"uint56\",\"name\":\"dayUnclaimedSatoshisTotal\",\"type\":\"uint56\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"beginDay\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"endDay\",\"type\":\"uint256\"}],\"name\":\"dailyDataRange\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"list\",\"type\":\"uint256[]\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"beforeDay\",\"type\":\"uint256\"}],\"name\":\"dailyDataUpdate\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"decimals\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"\",\"type\":\"uint8\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"subtractedValue\",\"type\":\"uint256\"}],\"name\":\"decreaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"globalInfo\",\"outputs\":[{\"internalType\":\"uint256[13]\",\"name\":\"\",\"type\":\"uint256[13]\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"globals\",\"outputs\":[{\"internalType\":\"uint72\",\"name\":\"lockedHeartsTotal\",\"type\":\"uint72\"},{\"internalType\":\"uint72\",\"name\":\"nextStakeSharesTotal\",\"type\":\"uint72\"},{\"internalType\":\"uint40\",\"name\":\"shareRate\",\"type\":\"uint40\"},{\"internalType\":\"uint72\",\"name\":\"stakePenaltyTotal\",\"type\":\"uint72\"},{\"internalType\":\"uint16\",\"name\":\"dailyDataCount\",\"type\":\"uint16\"},{\"internalType\":\"uint72\",\"name\":\"stakeSharesTotal\",\"type\":\"uint72\"},{\"internalType\":\"uint40\",\"name\":\"latestStakeId\",\"type\":\"uint40\"},{\"internalType\":\"uint128\",\"name\":\"claimStats\",\"type\":\"uint128\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"address\",\"name\":\"spender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"addedValue\",\"type\":\"uint256\"}],\"name\":\"increaseAllowance\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"merkleLeaf\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32[]\",\"name\":\"proof\",\"type\":\"bytes32[]\"}],\"name\":\"merkleProofIsValid\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"pure\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"pubKeyX\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"pubKeyY\",\"type\":\"bytes32\"},{\"internalType\":\"uint8\",\"name\":\"claimFlags\",\"type\":\"uint8\"}],\"name\":\"pubKeyToBtcAddress\",\"outputs\":[{\"internalType\":\"bytes20\",\"name\":\"\",\"type\":\"bytes20\"}],\"payable\":false,\"stateMutability\":\"pure\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"pubKeyX\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"pubKeyY\",\"type\":\"bytes32\"}],\"name\":\"pubKeyToEthAddress\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"pure\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"address\",\"name\":\"stakerAddr\",\"type\":\"address\"}],\"name\":\"stakeCount\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"stakeIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint40\",\"name\":\"stakeIdParam\",\"type\":\"uint40\"}],\"name\":\"stakeEnd\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"address\",\"name\":\"stakerAddr\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"stakeIndex\",\"type\":\"uint256\"},{\"internalType\":\"uint40\",\"name\":\"stakeIdParam\",\"type\":\"uint40\"}],\"name\":\"stakeGoodAccounting\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"stakeLists\",\"outputs\":[{\"internalType\":\"uint40\",\"name\":\"stakeId\",\"type\":\"uint40\"},{\"internalType\":\"uint72\",\"name\":\"stakedHearts\",\"type\":\"uint72\"},{\"internalType\":\"uint72\",\"name\":\"stakeShares\",\"type\":\"uint72\"},{\"internalType\":\"uint16\",\"name\":\"lockedDay\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"stakedDays\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"unlockedDay\",\"type\":\"uint16\"},{\"internalType\":\"bool\",\"name\":\"isAutoStake\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"newStakedHearts\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"newStakedDays\",\"type\":\"uint256\"}],\"name\":\"stakeStart\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transfer\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"recipient\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"transferFrom\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"xfLobby\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"address\",\"name\":\"referrerAddr\",\"type\":\"address\"}],\"name\":\"xfLobbyEnter\",\"outputs\":[],\"payable\":true,\"stateMutability\":\"payable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"address\",\"name\":\"memberAddr\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"entryId\",\"type\":\"uint256\"}],\"name\":\"xfLobbyEntry\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"rawAmount\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"referrerAddr\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"enterDay\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"count\",\"type\":\"uint256\"}],\"name\":\"xfLobbyExit\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":false,\"inputs\":[],\"name\":\"xfLobbyFlush\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"xfLobbyMembers\",\"outputs\":[{\"internalType\":\"uint40\",\"name\":\"headIndex\",\"type\":\"uint40\"},{\"internalType\":\"uint40\",\"name\":\"tailIndex\",\"type\":\"uint40\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"address\",\"name\":\"memberAddr\",\"type\":\"address\"}],\"name\":\"xfLobbyPendingDays\",\"outputs\":[{\"internalType\":\"uint256[2]\",\"name\":\"words\",\"type\":\"uint256[2]\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"beginDay\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"endDay\",\"type\":\"uint256\"}],\"name\":\"xfLobbyRange\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"list\",\"type\":\"uint256[]\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"}]", "0x2b591e99afe9f32eaa6214f7b7629768c40eeb39");
            _cacheExpirationSecs = int.TryParse(config["CacheExpirationSecs"], out var cacheExpSecs) 
                ? cacheExpSecs
                : 30;
            _blocklyticsApiKey = config["BlocklyticsApiKey"];
            if (string.IsNullOrEmpty(_blocklyticsApiKey))
                throw new ApplicationException("'BlocklyticsApiKey' must be defined in your configuration");
        }

        [FunctionName("market")]
        public async Task<IActionResult> GetMarketData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            try
            {
                if (PairsAreStale())
                    await UpdatePairs();

                return new OkObjectResult(_pairs);
            }
            catch (Exception e)
            {
                log.LogCritical(e, $"Caught an exception while processing blocklytics <=> uniswap summary data: {e.Message}");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("marketForTokenSymbol")]
        public async Task<IActionResult> GetMarketForTokenSymbol(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="market/{tokenSymbol}")] HttpRequest req,
            string tokenSymbol,
            ILogger log)
        {
            try
            {
                if (PairsAreStale())
                    await UpdatePairs();

                var pair = _pairs
                    .ToList()
                    .FirstOrDefault(p => p.TokenSymbol().Equals(tokenSymbol, StringComparison.InvariantCultureIgnoreCase));
                return new OkObjectResult(pair);
            }
            catch (Exception e)
            {
                log.LogCritical(e, $"Caught an exception while processing blocklytics <=> uniswap summary pair data: {e.Message}");
                return new InternalServerErrorResult();
            }
        }

        private async Task UpdatePairs()
        {
            var response =
                await _client.GetAsync(
                    $"https://api.blocklytics.org/uniswap/v1/exchanges?key={_blocklyticsApiKey}");

            var dtoString = await response.Content.ReadAsStringAsync();
            var blocklyticsPairs = JsonConvert.DeserializeObject<List<BlocklyticsUniswapPairsDTO>>(dtoString);
            _pairs = blocklyticsPairs
                .Where(pair => pair.Price != null && pair.Price.ToString() != "0")
                .Select(pair =>
                {
                    var uniswapPair = new UniswapPair
                    {
                        Timestamp = pair.Timestamp.ToUnixTimeMilliseconds(),
                        Pair = $"{pair.TokenSymbol}/ETH",
                        Price = (1 / (double)pair.Price).ToString(DecimalFormatter),
                        EthLiquidity = pair.EthLiquidity.ToString(DecimalFormatter),
                        TokenLiquidity = pair.TokenLiquidity.ToString(DecimalFormatter),
                        Volume24HrEth = pair.EthVolume.ToString(DecimalFormatter)
                    };

                    if (pair.TokenSymbol.Equals("hex", StringComparison.CurrentCultureIgnoreCase))
                        uniswapPair = UpdateHexUniswapPair(uniswapPair).Result;

                    return uniswapPair;
                })
                .Where(IsActive);
            _lastUpdated = DateTime.UtcNow;
        }

        // Temp fix for blocklytics liquidity pool flakiness
        private async Task<UniswapPair> UpdateHexUniswapPair(UniswapPair hexPair)
        {
            var heartBalanceResponse = await _hexContract.GetFunction("balanceOf").CallDecodingToDefaultAsync(HexUniswapAddress);
            var heartBalance = (BigInteger)heartBalanceResponse.First().Result;
            var hexBalance = (decimal)new BigDecimal(heartBalance, 8 * -1);

            var ethBalanceResponse = await _web3.Eth.GetBalance.SendRequestAsync(HexUniswapAddress);
            var ethBalance = Web3.Convert.FromWei(ethBalanceResponse.Value);

            if (ethBalance.Equals(0) || hexBalance.Equals(0))
                return hexPair;

            hexPair.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            hexPair.Price = (ethBalance / hexBalance).ToString(DecimalFormatter);
            hexPair.EthLiquidity = ethBalance.ToString(DecimalFormatter);
            hexPair.TokenLiquidity = hexBalance.ToString(DecimalFormatter);

            return hexPair;
        }

        private bool PairsAreStale()
        {
            return DateTime.UtcNow.Subtract(_lastUpdated) > TimeSpan.FromSeconds(_cacheExpirationSecs);
        }

        private static bool IsActive(UniswapPair pair) => pair.Price != "0" && pair.EthLiquidity != "0" &&
                                                          pair.TokenLiquidity != "0" && pair.Volume24HrEth != "0";
    }
}

package example;

import static io.gatling.javaapi.core.CoreDsl.*;
import static io.gatling.javaapi.http.HttpDsl.*;

import io.gatling.javaapi.core.*;
import io.gatling.javaapi.http.*;
import java.time.Duration;
import java.util.Iterator;
import java.util.Map;
import java.util.concurrent.ThreadLocalRandom;
import java.util.stream.Stream;

public class SchoolKeeperScaleComparisonSimulation extends Simulation {

  private static final String BASE_URL = System.getProperty("baseUrl", "http://localhost:8080");

  // Use a pre-generated JWT token to avoid auth endpoint bottleneck in scale comparison.
  private static final String AUTH_TOKEN = System.getProperty("authToken", "");

  private static final int WARMUP_SECONDS = Integer.getInteger("warmupSeconds", 30);
  private static final int RAMP_SECONDS = Integer.getInteger("rampSeconds", 60);
  private static final int HOLD_SECONDS = Integer.getInteger("holdSeconds", 180);
  private static final double TARGET_RPS = Double.parseDouble(System.getProperty("targetRps", "40"));

  private static final HttpProtocolBuilder HTTP_PROTOCOL =
      http.baseUrl(BASE_URL)
          .acceptHeader("application/json")
          .contentTypeHeader("application/json")
          .header("Authorization", "Bearer " + AUTH_TOKEN)
          .shareConnections();

  private static final Iterator<Map<String, Object>> PAGE_FEEDER =
      Stream.generate(
              () ->
                  Map.<String, Object>of(
                      "page", ThreadLocalRandom.current().nextInt(1, 6),
                      "pageSize", ThreadLocalRandom.current().nextInt(20, 101)))
          .iterator();

  private static final ChainBuilder READ_MIX =
      feed(PAGE_FEEDER)
          .randomSwitch()
          .on(
              percent(30.0).then(
                  exec(
                      http("GET /api/Statistics/overview")
                          .get("/api/Statistics/overview")
                          .check(status().is(200)))),
              percent(25.0).then(
                  exec(
                      http("GET /api/Statistics/incidents/trends")
                          .get("/api/Statistics/incidents/trends")
                          .check(status().is(200)))),
              percent(20.0).then(
                  exec(
                      http("GET /api/Statistics/devices/by-status")
                          .get("/api/Statistics/devices/by-status")
                          .check(status().is(200)))),
              percent(15.0).then(
                  exec(
                      http("GET /api/User")
                          .get("/api/User?page=#{page}&pageSize=#{pageSize}")
                          .check(status().is(200)))),
              percent(10.0).then(
                  exec(
                      http("GET /api/Device")
                          .get("/api/Device?page=#{page}&pageSize=#{pageSize}")
                          .check(status().is(200)))))
          .pause(Duration.ofMillis(50), Duration.ofMillis(200));

  private static final ScenarioBuilder SCENARIO =
      scenario("SchoolKeeper scale comparison (read-heavy)")
          .during(Duration.ofSeconds(WARMUP_SECONDS + HOLD_SECONDS))
          .on(READ_MIX);

  {
    if (AUTH_TOKEN.isBlank()) {
      throw new IllegalArgumentException(
          "authToken is required. Pass -DauthToken=<jwt> to run fair 3-vs-10 comparison.");
    }

    setUp(
            SCENARIO.injectOpen(
                rampUsersPerSec(1).to(TARGET_RPS).during(Duration.ofSeconds(RAMP_SECONDS)),
                constantUsersPerSec(TARGET_RPS).during(Duration.ofSeconds(HOLD_SECONDS))))
        .protocols(HTTP_PROTOCOL)
        .assertions(
            global().successfulRequests().percent().gt(95.0),
            global().responseTime().percentile4().lt(1500))
        .maxDuration(Duration.ofSeconds(RAMP_SECONDS + HOLD_SECONDS + 30));
  }
}

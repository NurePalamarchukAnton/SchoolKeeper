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

public class SchoolKeeperHeavyStressSimulation extends Simulation {

  private static final String BASE_URL = System.getProperty("baseUrl", "http://localhost:8080");
  private static final String LOGIN_EMAIL = System.getProperty("loginEmail", "admin@example.com");
  private static final String LOGIN_PASSWORD = System.getProperty("loginPassword", "123456");

  // Heavy profile controls
  private static final int HEAVY_USERS = Integer.getInteger("heavyUsers", 250);
  private static final int RAMP_SECONDS = Integer.getInteger("rampSeconds", 120);
  private static final int HOLD_SECONDS = Integer.getInteger("holdSeconds", 300);
  private static final double TARGET_RPS = Double.parseDouble(System.getProperty("targetRps", "180"));

  private static final HttpProtocolBuilder HTTP_PROTOCOL =
      http.baseUrl(BASE_URL)
          .acceptHeader("application/json")
          .contentTypeHeader("application/json")
          .userAgentHeader("Gatling Heavy Stress")
          .shareConnections();

  private static final Iterator<Map<String, Object>> PAGE_FEEDER =
      Stream.generate(
              () ->
                  Map.<String, Object>of(
                      "page", ThreadLocalRandom.current().nextInt(1, 6),
                      "pageSize", ThreadLocalRandom.current().nextInt(20, 81)))
          .iterator();

  private static final ChainBuilder LOGIN =
      exec(
              http("POST /api/Auth/login")
                  .post("/api/Auth/login")
                  .body(
                      StringBody(
                          "{\"email\":\"#{email}\",\"password\":\"#{password}\"}"))
                  .check(status().is(200))
                  .check(jsonPath("$.data.token").saveAs("jwtToken")))
          .exitHereIfFailed();

  private static final ChainBuilder BUSINESS_MIX =
      feed(PAGE_FEEDER)
          .exec(
              http("GET /api/Statistics/overview")
                  .get("/api/Statistics/overview")
                  .header("Authorization", "Bearer #{jwtToken}")
                  .check(status().is(200)))
          .exec(
              http("GET /api/Statistics/incidents/trends")
                  .get("/api/Statistics/incidents/trends")
                  .header("Authorization", "Bearer #{jwtToken}")
                  .check(status().is(200)))
          .exec(
              http("GET /api/Statistics/devices/by-status")
                  .get("/api/Statistics/devices/by-status")
                  .header("Authorization", "Bearer #{jwtToken}")
                  .check(status().is(200)))
          .exec(
              http("GET /api/User")
                  .get("/api/User?page=#{page}&pageSize=#{pageSize}")
                  .header("Authorization", "Bearer #{jwtToken}")
                  .check(status().is(200)))
          .exec(
              http("GET /api/Device")
                  .get("/api/Device?page=#{page}&pageSize=#{pageSize}")
                  .header("Authorization", "Bearer #{jwtToken}")
                  .check(status().is(200)))
          .pause(Duration.ofMillis(100), Duration.ofMillis(700));

  private static final ScenarioBuilder HEAVY_SCENARIO =
      scenario("SchoolKeeper heavy stress")
          .exec(session -> session.setAll(Map.of("email", LOGIN_EMAIL, "password", LOGIN_PASSWORD)))
          .exec(LOGIN)
          .during(Duration.ofSeconds(HOLD_SECONDS))
          .on(BUSINESS_MIX);

  {
    setUp(
            HEAVY_SCENARIO.injectOpen(
                rampUsers(HEAVY_USERS).during(Duration.ofSeconds(RAMP_SECONDS)),
                constantUsersPerSec(TARGET_RPS).during(Duration.ofSeconds(HOLD_SECONDS))))
        .protocols(HTTP_PROTOCOL)
        .assertions(
            global().successfulRequests().percent().gt(95.0),
            global().responseTime().percentile4().lt(3000),
            forAll().failedRequests().percent().lt(5.0))
        .maxDuration(Duration.ofSeconds(RAMP_SECONDS + HOLD_SECONDS + 60));
  }
}

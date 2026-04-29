package example;

import static io.gatling.javaapi.core.CoreDsl.*;
import static io.gatling.javaapi.http.HttpDsl.*;

import io.gatling.javaapi.core.*;
import io.gatling.javaapi.http.*;
import java.time.Duration;
import java.util.Map;

public class PyroSafeLoginLoadSimulation extends Simulation {

  private static final String BASE_URL = System.getProperty("baseUrl", "http://localhost:8085");
  private static final String LOGIN_EMAIL =
      System.getProperty("loginEmail", "anton.palamarchuk@nure.ua");
  private static final String LOGIN_PASSWORD = System.getProperty("loginPassword", "123456");

  private static final int USERS = Integer.getInteger("users", 200);
  private static final int RAMP_SECONDS = Integer.getInteger("rampSeconds", 60);
  private static final int HOLD_SECONDS = Integer.getInteger("holdSeconds", 120);

  private static final HttpProtocolBuilder HTTP_PROTOCOL =
      http.baseUrl(BASE_URL)
          .acceptHeader("application/json")
          .contentTypeHeader("application/json")
          .userAgentHeader("PyroSafe Login Load");

  private static final ChainBuilder LOGIN_REQUEST =
      exec(
              http("POST /api/users/login")
                  .post("/api/users/login")
                  .body(StringBody("{\"email\":\"#{email}\",\"password\":\"#{password}\"}"))
                  .check(status().is(200))
                  .check(jsonPath("$.success").is("true")))
          .pause(1);

  private static final ScenarioBuilder LOGIN_SCENARIO =
      scenario("PyroSafe login load")
          .exec(session -> session.setAll(Map.of("email", LOGIN_EMAIL, "password", LOGIN_PASSWORD)))
          .during(Duration.ofSeconds(HOLD_SECONDS))
          .on(LOGIN_REQUEST);

  {
    setUp(
            LOGIN_SCENARIO.injectClosed(
                rampConcurrentUsers(1).to(USERS).during(Duration.ofSeconds(RAMP_SECONDS)),
                constantConcurrentUsers(USERS).during(Duration.ofSeconds(HOLD_SECONDS))))
        .protocols(HTTP_PROTOCOL)
        .assertions(
            global().failedRequests().percent().lt(1.0),
            global().responseTime().percentile4().lt(2000))
        .maxDuration(Duration.ofSeconds(RAMP_SECONDS + HOLD_SECONDS + 15));
  }
}

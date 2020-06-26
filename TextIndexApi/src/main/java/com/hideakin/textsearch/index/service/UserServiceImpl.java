package com.hideakin.textsearch.index.service;

import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Base64;
import java.util.List;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.data.AuthenticateError;
import com.hideakin.textsearch.index.entity.UserEntity;
import com.hideakin.textsearch.index.exception.UnauthorizedException;
import com.hideakin.textsearch.index.model.AuthenticateResponse;
import com.hideakin.textsearch.index.repository.UserRepository;

@Service
public class UserServiceImpl implements UserService {

	public static final String HMAC_ALGO = "HmacSHA256";
	public static final Charset HMAC_CSET = StandardCharsets.UTF_8;

	private static final String BASIC_SP = "Basic ";
	private static final String BEARER_SP = "Bearer ";

	@PersistenceContext
	private EntityManager em;

	@Value("${textsearch.api-key-expiry-in:3600}")
    private int apiKeyExpiryIn = 3600;

	@Autowired
	private UserRepository userRepository;

	@Override
	public AuthenticateResponse verifyUsernamePassword(String authorization) {
		if (authorization.startsWith(BASIC_SP)) {
			String t = authorization.substring(BASIC_SP.length()).trim();
			byte[] b = Base64.getDecoder().decode(t);
			String ucp = new String(b, StandardCharsets.UTF_8);
			String[] tt = ucp.split(":");
			if (tt.length == 2) {
				return verifyUsernamePassword(tt[0], tt[1]);
			}
		}
		throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Malformed authorization header.");
	}

	@Override
	public AuthenticateResponse verifyUsernamePassword(String username, String password) {
		List<UserEntity> entities = userRepository.findAllByUsername(username);
		for (UserEntity entity : entities) {
			if (password.equals(entity.getPassword())) {
				AuthenticateResponse rsp = new AuthenticateResponse();
				rsp.setAccessToken(generateApiKey(entity, apiKeyExpiryIn));
				rsp.setExpiresIn(apiKeyExpiryIn);
				rsp.setTokenType("bearer");
				userRepository.save(entity);
				return rsp;
			}
		}
		throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Invalid credentials.");
	}
	
	@Override
	public void verifyApiKey(String authorization) {
		if (authorization == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "No authorization header.");
		}
		if (authorization.startsWith(BEARER_SP)) {
			String key = authorization.substring(BEARER_SP.length()).trim();
			List<UserEntity> entities = userRepository.findAllByApiKey(key);
			ZonedDateTime ct = ZonedDateTime.now();
			for (UserEntity entity : entities) {
				if (entity.getExpiry().isAfter(ct)) {
					return;
				}
			}
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, entities.size() > 0 ? "Expired API key." : "Invalid API key.");
		}
		throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Malformed authorization header.");
	}
	
	@Override
	public void verifyApiKey(String authorization, String role) {
		if (authorization == null) {
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "No authorization header.");
		}
		if (authorization.startsWith(BEARER_SP)) {
			String key = authorization.substring(BEARER_SP.length()).trim();
			List<UserEntity> entities = userRepository.findAllByApiKey(key);
			ZonedDateTime ct = ZonedDateTime.now();
			for (UserEntity entity : entities) {
				if (entity.getExpiry().isAfter(ct)) {
					String[] roles = entity.getRoles().split(",");
					for (String next : roles) {
						if (next.equalsIgnoreCase(role)) {
							return;
						}
					}
					throw new UnauthorizedException(AuthenticateError.ACCESS_DENIED, AuthenticateError.ACCESS_DENIED_DESC);
				}
			}
			throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, entities.size() > 0 ? "Expired API key." : "Invalid API key.");
		}
		throw new UnauthorizedException(AuthenticateError.INVALID_REQUEST, "Malformed authorization header.");
	}
	
	private String generateApiKey(UserEntity entity, int seconds) {
		String apiKey = null;
		entity.setExpiry(ZonedDateTime.now().plusSeconds(seconds));
		StringBuilder ibuf = new StringBuilder();
		ibuf.append(entity.getUsername());
		ibuf.append("|");
		ibuf.append(entity.getExpiry().format(DateTimeFormatter.ISO_DATE_TIME));
		byte[] input = ibuf.toString().getBytes(HMAC_CSET);
		byte[] key = entity.getPassword().getBytes(HMAC_CSET);
		SecretKeySpec sk = new SecretKeySpec(key, HMAC_ALGO);
		try {
			Mac mac = Mac.getInstance(HMAC_ALGO);
			mac.init(sk);
			byte[] hash = mac.doFinal(input);
			StringBuilder obuf = new StringBuilder(2 * hash.length);
			for (byte h : hash) {
				obuf.append(String.format("%02X", h & 0xff) );
			}
			apiKey = obuf.toString();
		} catch (NoSuchAlgorithmException e) {
			e.printStackTrace();
		} catch (InvalidKeyException e) {
			e.printStackTrace();
		}
		entity.setApiKey(apiKey);
		return apiKey;
	}
}

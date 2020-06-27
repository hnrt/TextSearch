package com.hideakin.textsearch.index.service;

import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.data.VerifyApiKeyResult;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.entity.UserEntity;
import com.hideakin.textsearch.index.model.AuthenticateResult;
import com.hideakin.textsearch.index.model.UserInfo;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.repository.UserRepository;
import com.hideakin.textsearch.index.utility.HmacSHA256;

@Service
@Transactional
public class UserServiceImpl implements UserService {

	private static final Logger logger = LoggerFactory.getLogger(UserServiceImpl.class);
	
	@PersistenceContext
	private EntityManager em;

	@Value("${textsearch.api-key.expiry-in:3600}")
    private int apiKeyExpiryIn = 3600;

	@Autowired
	private UserRepository userRepository;

	@Autowired
	private PreferenceRepository preferenceRepository;

	@Override
	public AuthenticateResult authenticate(String username, String password) {
		UserEntity entity = userRepository.findByUsername(username);
		if (entity != null) {
			String digest = digestPassword(username, password);
			if (digest.equals(entity.getPassword())) {
				String apiKey = generateApiKey(entity, apiKeyExpiryIn);
				userRepository.save(entity);
				return new AuthenticateResult(apiKey, apiKeyExpiryIn);
			}
		}
		return null;
	}
	
	private static String digestPassword(String username, String password) {
		final String SALT = "ah";
		return HmacSHA256.compute(password, SALT + username);
	}

	private static String generateApiKey(UserEntity entity, int seconds) {
		entity.setExpiry(ZonedDateTime.now().plusSeconds(seconds));
		StringBuilder ibuf = new StringBuilder();
		ibuf.append(entity.getUsername());
		ibuf.append("|");
		ibuf.append(entity.getExpiry().format(DateTimeFormatter.ISO_DATE_TIME));
		String apiKey = HmacSHA256.compute(ibuf.toString(), entity.getPassword());
		entity.setApiKey(apiKey);
		return apiKey;
	}
	
	@Override
	public VerifyApiKeyResult verifyApiKey(String key, String role) {
		List<UserEntity> entities = userRepository.findAllByApiKey(key);
		if (entities == null || entities.size() == 0) {
			return VerifyApiKeyResult.KeyNotFound;
		}
		ZonedDateTime ct = ZonedDateTime.now();
		for (UserEntity entity : entities) {
			if (entity.getExpiry().isAfter(ct)) {
				if (role != null) {
					String[] roles = entity.getRoles().split(",");
					for (String next : roles) {
						if (next.equalsIgnoreCase(role)) {
							return VerifyApiKeyResult.Success;
						}
					}
					return VerifyApiKeyResult.RoleMismatch;
				} else {
					return VerifyApiKeyResult.Success;
				}
			}
		}
		return VerifyApiKeyResult.KeyExpired;
	}

	@Override
	public UserInfo[] getUsers() {
		List<UserEntity> entities = userRepository.findAll();
		UserInfo[] users = new UserInfo[entities.size()];
		int index = 0;
		for (UserEntity entity : entities) {
			UserInfo ui = new UserInfo();
			ui.setUid(entity.getUid());
			ui.setUsername(entity.getUsername());
			ui.setRoles(entity.getRoles());
			ui.setCreatedAt(entity.getCreatedAt());
			ui.setUpdatedAt(entity.getUpdatedAt());
			ui.setExpiry(entity.getExpiry());
			ui.setApiKey(entity.getApiKey());
			users[index++] = ui;
		}
		return users;
	}

	@Override
	public UserInfo getUser(String username) {
		UserEntity entity = userRepository.findByUsername(username);
		if (entity != null) {
			UserInfo ui = new UserInfo();
			ui.setUid(entity.getUid());
			ui.setUsername(entity.getUsername());
			ui.setRoles(entity.getRoles());
			ui.setCreatedAt(entity.getCreatedAt());
			ui.setUpdatedAt(entity.getUpdatedAt());
			ui.setExpiry(entity.getExpiry());
			ui.setApiKey(entity.getApiKey());
			return ui;
		} else {
			return null;
		}
	}

	@Override
	public void createUser(String username, String password, String roles) {
		UserEntity entity = new UserEntity();
		entity.setUid(getNextUid());
		entity.setUsername(username);
		entity.setPassword(digestPassword(username, password));
		entity.setRoles(roles);
		ZonedDateTime ct = ZonedDateTime.now();
		entity.setCreatedAt(ct);
		entity.setUpdatedAt(ct);
		entity.setExpiry(null);
		entity.setApiKey(null);
		userRepository.saveAndFlush(entity);
		logger.info(String.format("createUser(%s|%s) succeeded.", username, roles));
	}
	
	private int getNextUid() {
		int next;
		final String name = "UID.next";
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			next = Integer.parseInt(entity.getValue(), 10);
		} else {
			entity = new PreferenceEntity();
			entity.setName(name);
			Integer maxUid = (Integer)em.createQuery("SELECT MAX(uid) FROM users").getSingleResult();
			next = (maxUid != null ? maxUid : 0) + 1;
		}
		entity.setValue(String.format("%d", next + 1));
		preferenceRepository.save(entity);
		return next;
	}

	@Override
	public boolean updateUser(String username, String password, String roles) {
		UserEntity entity = userRepository.findByUsername(username);
		if (entity == null) {
			return false;
		}
		if (password != null) {
			entity.setPassword(digestPassword(username, password));
		}
		if (roles != null) {
			entity.setRoles(roles);
		}
		entity.setUpdatedAt(ZonedDateTime.now());
		entity.setExpiry(null);
		entity.setApiKey(null);
		userRepository.saveAndFlush(entity);
		return true;
	}

	@Override
	public boolean deleteUser(String username) {
		UserEntity entity = userRepository.findByUsername(username);
		if (entity == null) {
			return false;
		}
		userRepository.deleteByUsername(username);
		return true;
	}

}

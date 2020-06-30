package com.hideakin.textsearch.index.service;

import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Arrays;
import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.entity.UserEntity;
import com.hideakin.textsearch.index.model.AuthenticateResult;
import com.hideakin.textsearch.index.model.UserInfo;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.repository.UserRepository;
import com.hideakin.textsearch.index.utility.HmacSHA256;
import com.hideakin.textsearch.index.utility.RoleComparator;
import com.hideakin.textsearch.index.utility.RolesIntersection;

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
	
	private final RoleComparator roleComp = new RoleComparator();

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
	public UserInfo[] getUsers() {
		List<UserEntity> entities = userRepository.findAll();
		UserInfo[] users = new UserInfo[entities.size()];
		int index = 0;
		for (UserEntity entity : entities) {
			users[index++] = new UserInfo(entity);
		}
		return users;
	}

	@Override
	public UserInfo getUser(String username) {
		UserEntity entity = userRepository.findByUsername(username);
		if (entity != null) {
			return new UserInfo(entity);
		} else {
			return null;
		}
	}

	@Override
	public UserInfo getUserByApiKey(String apiKey) {
		List<UserEntity> entities = userRepository.findAllByApiKey(apiKey);
		if (entities == null || entities.size() == 0) {
			return null;
		} else if (entities.size() > 0) {
			logger.warn("Found duplicate API keys: {}", apiKey);
			for (int index = 0; index < entities.size(); index++) {
				logger.warn("[{}] {}", index, entities.get(index).getUsername());
			}
		}
		return new UserInfo(entities.get(0));
	}

	@Override
	public UserInfo createUser(String username, String password, String[] roles) {
		Arrays.sort(roles, roleComp);
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
		return new UserInfo(entity);
	}
	
	private int getNextUid() {
		int nextId;
		final String name = "UID.next";
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			nextId = entity.getIntValue();
		} else {
			entity = new PreferenceEntity(name);
			Integer maxId = (Integer)em.createQuery("SELECT MAX(uid) FROM users").getSingleResult();
			nextId = (maxId != null ? maxId : 0) + 1;
		}
		entity.setValue(nextId + 1);
		preferenceRepository.save(entity);
		return nextId;
	}

	@Override
	public UserInfo updateUser(String username, String password, String[] roles) {
		UserEntity entity = userRepository.findByUsername(username);
		if (entity == null) {
			return null;
		}
		if (password != null) {
			entity.setPassword(digestPassword(username, password));
		}
		if (roles != null) {
			Arrays.sort(roles, roleComp);
			entity.setRoles(roles);
		}
		entity.setUpdatedAt(ZonedDateTime.now());
		entity.setExpiry(null);
		entity.setApiKey(null);
		userRepository.saveAndFlush(entity);
		return new UserInfo(entity);
	}

	@Override
	public UserInfo deleteUser(String username) {
		UserEntity entity = userRepository.findByUsername(username);
		if (entity == null) {
			return null;
		}
		userRepository.deleteByUsername(username);
		return new UserInfo(entity);
	}

}

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
import com.hideakin.textsearch.index.exception.ForbiddenException;
import com.hideakin.textsearch.index.exception.InvalidParameterException;
import com.hideakin.textsearch.index.model.AuthenticateResult;
import com.hideakin.textsearch.index.model.UserInfo;
import com.hideakin.textsearch.index.repository.PreferenceRepository;
import com.hideakin.textsearch.index.repository.UserRepository;
import com.hideakin.textsearch.index.utility.HmacSHA256;
import com.hideakin.textsearch.index.utility.RoleComparator;
import com.hideakin.textsearch.index.validator.UserNameValidator;

@Service
@Transactional
public class UserServiceImpl implements UserService {

	private static final Logger logger = LoggerFactory.getLogger(UserServiceImpl.class);
	
	@PersistenceContext
	private EntityManager em;

	@Value("${textsearch.access-token.expires-in:3600}")
    private int accessTokenExpiresIn = 3600; // seconds

	@Autowired
	private UserRepository userRepository;

	@Autowired
	private PreferenceRepository preferenceRepository;
	
	private final RoleComparator roleComp = new RoleComparator();

	@Override
	public AuthenticateResult authenticate(String username, String password) {
		UserEntity entity = userRepository.findByUsername(username);
		if (entity == null) {
			return null;
		}
		String digest = digestPassword(username, password);
		if (!digest.equals(entity.getPassword())) {
			return null;
		}
		generateAccessToken(entity, accessTokenExpiresIn);
		userRepository.save(entity);
		return new AuthenticateResult(entity.getAccessToken(), accessTokenExpiresIn);
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
	public UserInfo getUser(int uid) {
		UserEntity entity = userRepository.findByUid(uid);
		return entity != null ? new UserInfo(entity) : null;
	}

	@Override
	public UserInfo getUser(String username) {
		UserEntity entity = userRepository.findByUsername(username);
		return entity != null ? new UserInfo(entity) : null;
	}

	@Override
	public UserInfo getUserByAccessToken(String accessToken) {
		UserEntity entity = userRepository.findByAccessToken(accessToken);
		return entity != null ? new UserInfo(entity) : null;
	}

	@Override
	public UserInfo createUser(String username, String password, String[] roles) {
		if (!UserNameValidator.isValidUsername(username)) {
			throw new InvalidParameterException("Invalid username.");
		}
		if (!UserNameValidator.areValidRoles(roles)) {
			throw new InvalidParameterException("Invalid role.");
		}
		Arrays.sort(roles, roleComp);
		ZonedDateTime ct = ZonedDateTime.now();
		UserEntity entity = new UserEntity();
		entity.setUid(getNextUid());
		entity.setUsername(username);
		entity.setPassword(digestPassword(username, password));
		entity.setRoles(roles);
		entity.setCreatedAt(ct);
		entity.setUpdatedAt(ct);
		entity.setAccessToken(null);
		entity.setExpiresAt(null);
		userRepository.saveAndFlush(entity);
		logger.info("createUser([{}] {}) succeeded.", entity.getUid(), entity.getUsername());
		return new UserInfo(entity);
	}
	
	@Override
	public UserInfo updateUser(int uid, String username, String password, String[] roles) {
		UserEntity entity = userRepository.findByUid(uid);
		if (entity == null) {
			return null;
		}
		if (username != null) {
			if (!UserNameValidator.isValidUsername(username)) {
				throw new InvalidParameterException("Invalid username.");
			}
			entity.setUsername(username);
		}
		if (password != null) {
			entity.setPassword(digestPassword(username, password));
		}
		if (roles != null) {
			if (!UserNameValidator.areValidRoles(roles)) {
				throw new InvalidParameterException("Invalid role.");
			}
			Arrays.sort(roles, roleComp);
			entity.setRoles(roles);
		}
		entity.setUpdatedAt(ZonedDateTime.now());
		entity.setAccessToken(null);
		entity.setExpiresAt(null);
		userRepository.saveAndFlush(entity);
		logger.info("updateUser([{}] {}) succeeded.", entity.getUid(), entity.getUsername());
		return new UserInfo(entity);
	}

	@Override
	public UserInfo deleteUser(int uid) {
		if (uid == 0) {
			throw new ForbiddenException("Not allowed to delete the user of UID=0.");
		}
		UserEntity entity = userRepository.findByUid(uid);
		if (entity == null) {
			return null;
		}
		userRepository.delete(entity);
		logger.info("deleteUser([{}] {}) succeeded.", entity.getUid(), entity.getUsername());
		return new UserInfo(entity);
	}
	
	private static String digestPassword(String username, String password) {
		final String SALT = "ah";
		return HmacSHA256.compute(password, SALT + username);
	}

	private static void generateAccessToken(UserEntity entity, int seconds) {
		entity.setExpiresAt(ZonedDateTime.now().plusSeconds(seconds));
		StringBuilder ibuf = new StringBuilder();
		ibuf.append(entity.getUsername());
		ibuf.append("|");
		ibuf.append(entity.getExpiresAt().format(DateTimeFormatter.ISO_DATE_TIME));
		String accessToken = HmacSHA256.compute(ibuf.toString(), entity.getPassword());
		entity.setAccessToken(accessToken);
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

}

package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@SpringBootTest
public class PreferenceServiceTests {

	@Mock
	private PreferenceRepository preferenceRepository;

	@InjectMocks
	private PreferenceService preferenceService = new PreferenceServiceImpl();

	@Test
	public void getPreference_successful() {
		when(preferenceRepository.findByName("quux")).thenReturn(new PreferenceEntity("quux", "fred"));
		String value = preferenceService.getPreference("quux");
		Assertions.assertEquals("fred", value);
	}

	@Test
	public void getPreference_notfound() {
		when(preferenceRepository.findByName("quux")).thenReturn(null);
		String value = preferenceService.getPreference("quux");
		Assertions.assertEquals(null, value);
	}

	@Test
	public void createPreference_successful() {
		when(preferenceRepository.save(argThat(x -> x.getName().equals("foo") && x.getValue().equals("bar")))).thenReturn(new PreferenceEntity("foo", "bar"));
		preferenceService.createPreference("foo", "bar");
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("foo") && x.getValue().equals("bar")));
	}

	@Test
	public void updatePreference_successful() {
		when(preferenceRepository.findByName("foo")).thenReturn(new PreferenceEntity("foo", "bar"));
		boolean ret = preferenceService.updatePreference("foo", "baz");
		Assertions.assertEquals(true, ret);
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("foo") && x.getValue().equals("baz")));
	}

	@Test
	public void updatePreference_notFound() {
		when(preferenceRepository.findByName("foo")).thenReturn(null);
		boolean ret = preferenceService.updatePreference("foo", "baz");
		Assertions.assertEquals(false, ret);
		verify(preferenceRepository, times(0)).save(any(PreferenceEntity.class));
	}

	@Test
	public void deletePreference_successful() {
		when(preferenceRepository.findByName("quux")).thenReturn(new PreferenceEntity("quux", "fred"));
		boolean ret = preferenceService.deletePreference("quux");
		Assertions.assertEquals(true, ret);
		verify(preferenceRepository, times(1)).delete(argThat(x -> x.getName().equals("quux") && x.getValue().equals("fred")));
	}

	@Test
	public void deletePreference_notfound() {
		when(preferenceRepository.findByName("quux")).thenReturn(null);
		boolean ret = preferenceService.deletePreference("quux");
		Assertions.assertEquals(false, ret);
		verify(preferenceRepository, times(0)).delete(any(PreferenceEntity.class));
	}
	
	@Test
	public void isServiceUnavailable_true() {
		when(preferenceRepository.findByName("enabled")).thenReturn(new PreferenceEntity("enabled", "false"));
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(true, ret);
		verify(preferenceRepository, times(1)).findByName("enabled");
	}
	
	@Test
	public void isServiceUnavailable_false() {
		when(preferenceRepository.findByName("enabled")).thenReturn(new PreferenceEntity("enabled", "true"));
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
		verify(preferenceRepository, times(1)).findByName("enabled");
	}
	
	@Test
	public void isServiceUnavailable_invalid() {
		when(preferenceRepository.findByName("enabled")).thenReturn(new PreferenceEntity("enabled", "xyzzy"));
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
		verify(preferenceRepository, times(1)).findByName("enabled");
	}
	
	@Test
	public void isServiceUnavailable_null() {
		when(preferenceRepository.findByName("enabled")).thenReturn(null);
		boolean ret = preferenceService.isServiceUnavailable();
		Assertions.assertEquals(false, ret);
		verify(preferenceRepository, times(1)).findByName("enabled");
	}

	@Test
	public void setServiceAvailability_true() {
		preferenceService.setServiceAvailability(true);
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("enabled") && x.getValue().equals("true")));
	}

	@Test
	public void setServiceAvailability_false() {
		preferenceService.setServiceAvailability(false);
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("enabled") && x.getValue().equals("false")));
	}

}
